using System;
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
        [Authorize]
        [Route("api/Account/Get")]
        public async Task<IHttpActionResult> Get()
        {
            // Registered User
            if (await AppDatabase.Users.AnyAsync(o => o.Id == UserId))
                return BadRequest("UserId is in use. Try password reset.");

            var user = await AppDatabase.Users.FirstOrDefaultAsync(o => o.Id == UserId);

            if (user == null)
            {
                return Unauthorized();
            }

            Authorization.UpdateFrom(user);
            Session.UpdateFrom(user);

            return Ok(GetAccountDetails());
        }

        /// <summary>
        /// Creates a new User
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Account/Guest")]
        public async Task<IHttpActionResult> Guest(AccountGuestSignIn model)
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
        public async Task<IHttpActionResult> SignIn(AccountEmailSignIn model)
        {
            var user = await AppDatabase.Users.FirstOrDefaultAsync(o => o.Email == model.Email);

            if (user == null)
                return await Create(model);

            if (!user.EmailPassword.Compare(model.Password))
                return BadRequest("Invalid password.");

            //Sign In
            Authorization.UpdateFrom(user);
            Session.UpdateFrom(user);

            return Ok(GetAccountDetails());
        }

        async Task<IHttpActionResult> Create(AccountEmailSignIn model)
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

            var user = new UserAccount
            {
                Email = model.Email,
                Id = model.UserId,
                CreatedOn = DateTime.UtcNow,
                ModifiedOn = DateTime.UtcNow,
                EmailPassword = UserPassword.Create(model.Password),
                PhonePassword = UserPassword.Create(model.Password),
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
        [Route("api/Account/Reset")]
        public async Task<IHttpActionResult> Reset(AccountEmailReset model)
        {
            var user = await AppDatabase.Users.FirstOrDefaultAsync(o => o.Email == model.Email);

            if (user == null)
                return BadRequest("User not found");

            if (string.IsNullOrEmpty(user.Email))
                return BadRequest("User lacks a valid email address. Did you use Facebook or Twilio ?");

            var token = Strings.RandomString(6).ToLower();

            user.ModifiedOn = DateTime.UtcNow;
            user.EmailPassword = UserPassword.Create(token);

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
        /// Updates the user's details.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("api/Account/Update")]
        public async Task<IHttpActionResult> Update(AccountEmailUpdate model)
        {
            var user = await AppDatabase.Users.FindAsync(Session.UserId);

            // no user? signUp. This happens from guest upgrades.
            if (user == null)
            {
                return await Create(new AccountEmailSignIn
                {
                    UserId = Session.UserId,
                    Email = model.NewEmail,
                    Password = model.NewPassword
                });
            }

            // make sure Email is not in use by someone else
            string oldEmail = null;
            if (!string.IsNullOrEmpty(model.NewEmail))
            {
                var users = await AppDatabase.Users
                    .Where(o => o.Id != Session.UserId)
                    .AnyAsync(o => o.Email == model.NewEmail);

                if (users)
                    return BadRequest("Email is in use.");

                oldEmail = user.Email;
                user.Email = model.NewEmail;
            }

            // update password
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                user.EmailPassword = UserPassword.Create(model.NewPassword);
            }

            // update password
            if (!string.IsNullOrEmpty(model.NewEmail))
            {
                user.Email = model.NewEmail;
            }

            // update
            user.ModifiedOn = DateTime.UtcNow;
            AppDatabase.Entry(user).State = EntityState.Modified;
            await AppDatabase.SaveChangesAsync();

            await SendUpdateEmail(new UserUpdateViewModel
            {
                UserId = user.Id,
                UserEmail = user.Email,
                OldEmail = oldEmail,

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
        public async Task<IHttpActionResult> Delete(AccountEmailDelete model)
        {
            var user = await AppDatabase.Users.FindAsync(Authorization.UserId);

            if (user == null)
                return BadRequest("User not found");

            if (!user.EmailPassword.Compare(model.Password))
                return BadRequest("Invalid password.");
            
            // delete
            await DeleteUser(user);

            Session.SignOut();
            Authorization.SignOut();

            return Ok();
        }
        #endregion

        #region Facebook
        /// <summary>
        /// Creates a new User
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Account/FacebookConnect")]
        public async Task<IHttpActionResult> FacebookConnect(AccountFacebookConnect model)
        {
            if (!Authorization.IsAuthenticated)
            {
                return await FacebookCreate(model);
            }

            return await FacebookUpdate(model);
        }

        public async Task<IHttpActionResult> FacebookCreate(AccountFacebookConnect model)
        {
            var client = new FacebookClient(model.AccessToken);
            client.AppId = AppConfig.Settings.FacebookId;
            client.AppSecret = AppConfig.Settings.FacebookSecret;

            dynamic fbresult = client.Get("me?fields=id,email,first_name,last_name,gender,locale,link,timezone,location,picture");
            string email = fbresult.email;

            var social = await AppDatabase.UserFacebookClaims.FindAsync(fbresult.id);

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
            var user = new UserAccount
            {
                CreatedOn = DateTime.UtcNow,
                ModifiedOn = DateTime.UtcNow,
                Email = email,
                Id = Guid.NewGuid().ToString(),
                EmailPassword = UserPassword.Create(password),

            };
            AppDatabase.Users.Add(user);

            social = new UserFacebookClaim
            {
                Id = fbresult.id,
                UserId = user.Id,
                User = user,
                AccessToken = model.AccessToken
            };

            FacebookUpdateInternal(social, fbresult);

            AppDatabase.UserFacebookClaims.Add(social);

            await SendWelcomeMail(new UserEmailViewModel
            {
                UserId = user.Id,
                UserEmail = user.Email
            });

            await AppDatabase.SaveChangesAsync();

            Session.UpdateFrom(user);
            return Ok(GetAccountDetails());
        }

        public async Task<IHttpActionResult> FacebookUpdate(AccountFacebookConnect model)
        {
            var client = new FacebookClient(model.AccessToken);
            client.AppId = AppConfig.Settings.FacebookId;
            client.AppSecret = AppConfig.Settings.FacebookSecret;

            dynamic fbresult = client.Get("me?fields=id,email,first_name,last_name,gender,locale,link,timezone,location,picture");

            var social = await AppDatabase.UserFacebookClaims.FindAsync(fbresult.id);
            var user = await AppDatabase.Users.FindAsync(UserId);

            if (social == null)
            {
                social = new UserFacebookClaim();
                social.Id = fbresult.id;
                social.User = user;
                social.UserId = UserId;
                social.AccessToken = model.AccessToken;
                social.Provider = APIConstants.FACEBOOK;
                AppDatabase.UserFacebookClaims.Add(social);
            }

            FacebookUpdateInternal(social, fbresult);

            await AppDatabase.SaveChangesAsync();
            Session.UpdateFrom(user);
            return Ok(GetAccountDetails());
        }

        void FacebookUpdateInternal(UserFacebookClaim social, dynamic fbresult)
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
        [Route("api/Account/FacebookDisconnect")]
        public async Task<IHttpActionResult> FacebookDisconnect(AccountFacebookDisconnect model)
        {
            var user = await AppDatabase.Users.Include(o => o.UserFacebookClaims).FirstOrDefaultAsync(o => o.Id == UserId);
            if (user == null)
                return BadRequest("User not found");


            var social = user.UserFacebookClaims.FirstOrDefault();
            if (social == null)
                return BadRequest("Social connection not found");

            if (user.UserFacebookClaims.Count() == 1 && string.IsNullOrEmpty(user.Email))
                return BadRequest("Orphan Account. Please add an email.");

            AppDatabase.UserFacebookClaims.Remove(social);
            await AppDatabase.SaveChangesAsync();

            return Ok(GetAccountDetails());
        }

        #endregion

        async Task DeleteUser(UserAccount user)
        {
            // delete
            AppDatabase.Entry(user).State = EntityState.Deleted;
            AppDatabase.Users.Remove(user);
            AppDatabase.UserFacebookClaims.RemoveRange(AppDatabase.UserFacebookClaims.Where(o => o.UserId == user.Id));

            AppDatabase.Storage.RemoveRange(AppDatabase.Storage.Where(o => o.AclParam == user.Id));
            AppDatabase.Scores.RemoveRange(AppDatabase.Scores.Where(o => o.UserId == user.Id));

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
                FacebookId = Session.FacebookId,
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