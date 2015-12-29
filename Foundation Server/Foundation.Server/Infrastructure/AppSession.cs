using System.Linq;
using Foundation.Server.Models;

namespace Foundation.Server.Infrastructure
{
    /// <summary>
    /// Application Session. Saved as an encrypted header cookie.
    /// </summary>
    /// <remarks>
    /// Used to preserve infrequently changing user data server-side
    /// </remarks>
    public partial class AppSession
    {
        //TODO put custom fields here

        public string Email { get; set; }

        public string FacebookId { get; set; }

        /// <summary>
        ///  TODO Update Custom Session fields here
        /// </summary>
        /// <param name="user"></param>
        protected void OnUpdate(UserAccount user)
        {
            Email = user.Email;
            if (user.UserFacebookClaims != null && user.UserFacebookClaims.Any())
            {
                var social = user.UserFacebookClaims.First();
                FacebookId = social.Id;
            }
        }

        /// <summary>
        ///  TODO Clean Custom Session fields here
        /// </summary>
        protected void OnDelete()
        {
            FacebookId = Email = string.Empty;
        }

    }
}