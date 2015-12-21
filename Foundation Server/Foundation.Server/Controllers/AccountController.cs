using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Http;
using Facebook;
using Foundation.Server.Api;
using Foundation.Server.Infrastructure;
using Foundation.Server.Infrastructure.Filters;
using Foundation.Server.Infrastructure.Helpers;
using Foundation.Server.Models;

namespace Foundation.Server.Controllers
{
    /// <summary>
    /// Account Services for the Game-play User
    /// </summary>
    [ApiValidateModelState]
    public class ApiAccountController : ApiControllerBase
    {
        public string AesKey
        {
            get { return AppConfig.Settings.AesKey; }
        }


        #region Guest / Device

        /// <summary>
        /// Creates a new User
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Account/Guest")]
        public async Task<IHttpActionResult> Guest(AccountGuestRequest model)
        {
            // Registered User
            if (await AppDatabase.Users.AnyAsync(o => o.Id == model.UserId && !string.IsNullOrEmpty(o.Email)))
                return BadRequest("UserId is in use. Try password reset.");

            //Custom SignIn
            Authorization.IsAuthenticated = true;
            Authorization.UserId = model.UserId;

            return Ok(GetAccountDetails());
        }

        
        #endregion

        #region Email

        /// <summary>
        /// Signs the existing user in.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Account/SignIn")]
        public async Task<IHttpActionResult> SignIn(AccountEmailRequest model)
        {
            var user = await AppDatabase.Users.FirstOrDefaultAsync(o => o.Email == model.Email);

            if (user == null)
                return await Create(model);

            var password = AESHelper.Decrypt(user.PasswordHash, AesKey);

            if (password != model.Password)
                return BadRequest("Invalid password.");

            //Sign In
            Authorization.UpdateFrom(user);
            Session.UpdateFrom(user);

            return Ok(GetAccountDetails());
        }

        public async Task<IHttpActionResult> Create(AccountEmailRequest model)
        {
            if (string.IsNullOrEmpty(model.UserId))
                model.UserId = Guid.NewGuid().ToString();

            if (await AppDatabase.Users.AnyAsync(o => o.Id == model.UserId))
            {
                return BadRequest("UserId is in use.");
            }
            if (await AppDatabase.Users.AnyAsync(o => o.Email == model.Email))
            {
                return BadRequest("Email is in use.");
            }

            var user = new AppUser
            {
                PasswordHash = AESHelper.Encrypt(model.Password, AesKey),
                Email = model.Email,
                Id = model.UserId,
                CreatedOn = DateTime.UtcNow,
                ModifiedOn = DateTime.UtcNow,
                ResetTokenExpires = DateTime.UtcNow,
            };

            AppDatabase.Users.Add(user);

            await AppDatabase.SaveChangesAsync();

            await SendWelcomeMail(new UserEmailViewModel
            {
                UserId = user.Id,
                UserEmail = user.Email
            });


            //Sign In
            Authorization.UpdateFrom(user);
            Session.UpdateFrom(user);

            return Ok(GetAccountDetails());
        }

        /// <summary>
        /// Issues a Token via email. The token is required for Update
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ApiValidateModelState]
        [Route("api/Account/Recover")]
        public async Task<IHttpActionResult> Recovery(AccountRecoverRequest model)
        {
            var user = await AppDatabase.Users.FirstOrDefaultAsync(o => o.Email == model.Email);

            if (user == null)
                return BadRequest("User not found");

            if (string.IsNullOrEmpty(user.Email))
                return BadRequest("User lacks a valid email address. Did you use Facebook ?");

            var token = Strings.RandomString(6).ToLower();

            user.ModifiedOn = DateTime.UtcNow;
            user.ResetToken = token;
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(24);

            AppDatabase.Entry(user).State = EntityState.Modified;

            await AppDatabase.SaveChangesAsync();

            await SendTokenEmail(new UserTokenViewModel
            {
                UserId = user.Id,
                UserEmail = user.Email,
                Token = token,
            });

            return Ok(GetAccountDetails());
        }

        /// <summary>
        /// Updates the user's details using the recovery token
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Account/Reset")]
        public async Task<IHttpActionResult> Reset(AccountEmailResetRequest model)
        {
            var user = await AppDatabase.Users.FirstOrDefaultAsync(o => o.ResetToken == model.Token && o.ResetTokenExpires > DateTime.UtcNow);

            if (user == null)
                return BadRequest("User not found or token is invalid");

            // update password
            if (!string.IsNullOrEmpty(model.Password))
                user.PasswordHash = AESHelper.Encrypt(model.Password, AesKey);

            // update
            user.ModifiedOn = DateTime.UtcNow;
            user.ResetTokenExpires = DateTime.UtcNow;
            AppDatabase.Entry(user).State = EntityState.Modified;
            await AppDatabase.SaveChangesAsync();

            //update session
            Authorization.UpdateFrom(user);
            Session.UpdateFrom(user);

            return Ok(GetAccountDetails());
        }

        /// <summary>
        /// Updates the user's details.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("api/Account/Update")]
        public async Task<IHttpActionResult> Update(AccountEmailUpdateRequest model)
        {
            var user = await AppDatabase.Users.FindAsync(Session.UserId);

            // no user? signUp. This happens from guest mode.
            if (user == null)
            {
                return await Create(new AccountEmailRequest
                {
                    UserId = Session.UserId,
                    Email = model.Email,
                    Password = model.Password
                });
            }

            // make sure Email is not in use by someone else
            string oldEmail = null;
            if (!string.IsNullOrEmpty(model.Email))
            {
                var users = await AppDatabase.Users
                    .Where(o => o.Id != Session.UserId)
                    .AnyAsync(o => o.Email == model.Email);

                if (users)
                    return BadRequest("Email is in use.");

                oldEmail = user.Email;
                user.Email = model.Email;
            }

            // update password
            if (!string.IsNullOrEmpty(model.Password))
                user.PasswordHash = AESHelper.Encrypt(model.Password, AesKey);

            // update
            user.ModifiedOn = DateTime.UtcNow;
            user.ResetTokenExpires = DateTime.UtcNow;
            AppDatabase.Entry(user).State = EntityState.Modified;
            await AppDatabase.SaveChangesAsync();

            await SendUpdateEmail(new UserUpdateViewModel
            {
                UserId = user.Id,
                UserEmail = user.Email,
                OldEmail = oldEmail
            });


            //update session
            Session.UpdateFrom(user);
            Authorization.UpdateFrom(user);

            return Ok(GetAccountDetails());

        }

        /// <summary>
        /// Deletes the user's details.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("api/Account/Delete")]
        public async Task<IHttpActionResult> Delete(AccountEmailDeleteRequest model)
        {
            var user = await AppDatabase.Users.FindAsync(model.UserId);

            if (user == null)
                return BadRequest("User not found");

            var password = AESHelper.Decrypt(user.PasswordHash, AesKey);
            if (password != model.Password)
                return BadRequest("Invalid password.");

            if (!model.Email.Equals(user.Email, StringComparison.CurrentCultureIgnoreCase))
                return BadRequest("Invalid email.");
            
            // delete
            await DeleteUser(user);

            return Ok(GetAccountDetails());
        }
        #endregion

        #region Facebook
        /// <summary>
        /// Creates a new User
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Account/Facebook")]
        public async Task<IHttpActionResult> Facebook(AccountFacebookRequest model)
        {
            if (!AppConfig.Settings.FacebookEnabled)
                return BadRequest("Social Signin disabled.");
            
            if (!Authorization.IsAuthenticated)
            {
                return await FacebookCreate(model);
            }

            return await FacebookUpdate(model);
        }

        public async Task<IHttpActionResult> FacebookCreate(AccountFacebookRequest model)
        {
            var client = new FacebookClient(model.AccessToken);
            client.AppId = AppConfig.Settings.FacebookId;
            client.AppSecret = AppConfig.Settings.FacebookSecret;

            dynamic fbresult = client.Get("me?fields=id,email,first_name,last_name,gender,locale,link,timezone,location,picture");
            string email = fbresult.email;

            var social = await AppDatabase.Socials.FindAsync(fbresult.id);

            if (social != null)
            {
                // old profile
                FacebookUpdateInternal(social, fbresult);
                await AppDatabase.SaveChangesAsync();

                var oldUser = social.User;
                Session.UpdateFrom(oldUser);
                return Ok(GetAccountDetails());
            }

            //email in use ?
            var user3 = await AppDatabase.Users.FirstOrDefaultAsync(o => o.Email == email);
            if (user3 != null)
            {
                return BadRequest("Email is in use. Try account recovery.");
            }

            // new user
            var password = new string(Guid.NewGuid().ToString().Take(7).ToArray());
            var user = new AppUser
            {
                CreatedOn = DateTime.UtcNow,
                ModifiedOn = DateTime.UtcNow,
                Email = email,
                Id = Guid.NewGuid().ToString(),
                PasswordHash = AESHelper.Encrypt(password, AesKey),

            };
            AppDatabase.Users.Add(user);

            social = new SocialProfile
            {
                Id = fbresult.id,
                UserId = user.Id,
                User = user,
                Provider = AccountConstants.FACEBOOK,
                AccessToken = model.AccessToken
            };

            FacebookUpdateInternal(social, fbresult);

            AppDatabase.Socials.Add(social);

            await SendWelcomeMail(new UserEmailViewModel
            {
                UserId = user.Id,
                UserEmail = user.Email
            });

            await AppDatabase.SaveChangesAsync();

            Session.UpdateFrom(user);
            return Ok(GetAccountDetails());
        }

        public async Task<IHttpActionResult> FacebookUpdate(AccountFacebookRequest model)
        {
            var client = new FacebookClient(model.AccessToken);
            client.AppId = AppConfig.Settings.FacebookId;
            client.AppSecret = AppConfig.Settings.FacebookSecret;

            dynamic fbresult = client.Get("me?fields=id,email,first_name,last_name,gender,locale,link,timezone,location,picture");

            var social = await AppDatabase.Socials.FindAsync(fbresult.id);
            var user = await AppDatabase.Users.FindAsync(UserId);

            if (social == null)
            {
                social = new SocialProfile();
                social.Id = fbresult.id;
                social.User = user;
                social.UserId = UserId;
                social.AccessToken = model.AccessToken;
                social.Provider = AccountConstants.FACEBOOK;
                AppDatabase.Socials.Add(social);
            }

            FacebookUpdateInternal(social, fbresult);

            await AppDatabase.SaveChangesAsync();
            Session.UpdateFrom(user);
            return Ok(GetAccountDetails());
        }

        void FacebookUpdateInternal(SocialProfile social, dynamic fbresult)
        {
            social.Email = fbresult.email;
            social.FirstName = fbresult.first_name;
            social.LastName = fbresult.last_name;
            social.Link = fbresult.link;
            social.Picture = String.Format("http://graph.facebook.com/{0}/picture", fbresult.id);
            social.Gender = fbresult.gender;
            social.Local = fbresult.locale;
        }

        /// <summary>
        /// Creates a new User
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("api/Account/FacebookDelete")]
        public async Task<IHttpActionResult> FacebookDelete(AccountFacebookDeleteRequest model)
        {
            var user = await AppDatabase.Users.Include(o => o.Socials).FirstOrDefaultAsync(o => o.Id == UserId);
            if (user == null)
                return BadRequest("User not found");


            var social = user.Socials.FirstOrDefault(o => o.Provider == AccountConstants.FACEBOOK);
            if (social == null)
                return BadRequest("Social connection not found");

            if (user.Socials.Count() == 1 && string.IsNullOrEmpty(user.Email))
                return BadRequest("Orphan Account. Please add an email.");

            AppDatabase.Socials.Remove(social);
            await AppDatabase.SaveChangesAsync();

            return Ok(GetAccountDetails());
        }

        #endregion
        
        public async Task DeleteUser(AppUser user)
        {
            // delete
            AppDatabase.Entry(user).State = EntityState.Deleted;
            AppDatabase.Objects.RemoveRange(AppDatabase.Objects.Where(o => o.AclParam == user.Id));
            AppDatabase.Socials.RemoveRange(AppDatabase.Socials.Where(o => o.UserId == user.Id));

            await AppDatabase.SaveChangesAsync();

            Session.SignOut();
            Authorization.SignOut();
        }

        #region helpers

        AccountDetails GetAccountDetails()
        {
            return new AccountDetails
                 {
                     //For client reading
                     Id = Session.UserId,
                     Email = Session.Email,
                     IsAuthenticated = Authorization.IsAuthenticated,
                     FacebookId =  Session.FacebookId,

                     //Header Data
                     AuthorizationToken = Authorization.ToAesJson(),
                     SessionToken = Session.ToAesJson()
            };
        }

        async Task SendWelcomeMail(UserEmailViewModel model)
        {
            if (string.IsNullOrEmpty(model.UserEmail))
                return;

            try
            {
                var mailUser = new MailAddress(model.UserEmail);
                var subject = String.Format("{0}, Welcome !", AppConfig.Settings.AppName);
                await Mailer.SendAsync("UserWelcome", model, subject, mailUser);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        async Task SendTokenEmail(UserTokenViewModel model)
        {
            try
            {
                var mailUser = new MailAddress(model.UserEmail);
                var subject = String.Format("{0}, Password Reset !", AppConfig.Settings.AppName);
                await Mailer.SendAsync("UserToken", model, subject, mailUser);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        async Task SendUpdateEmail(UserUpdateViewModel model)
        {
            try
            {
                var mailUser = new MailAddress(model.UserEmail);
                var subject = String.Format("{0}, Account Updated !", AppConfig.Settings.AppName);
                await Mailer.SendAsync("UserUpdate", model, subject, mailUser);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            try
            {
                if (!string.IsNullOrEmpty(model.OldEmail))
                {
                    var mailUser = new MailAddress(model.OldEmail);
                    var subject = String.Format("{0}, Account Updated !", AppConfig.Settings.AppName);
                    await Mailer.SendAsync("UserUpdate", model, subject, mailUser);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        #endregion
    }
}