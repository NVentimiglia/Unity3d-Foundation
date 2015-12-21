using System;
using System.Linq;
using Foundation.Server.Api;

namespace Foundation.Server.Models
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
        protected void OnUpdate(AppUser user)
        {
            Email = user.Email;
            var social = user.Socials.FirstOrDefault(o => o.Provider == AccountConstants.FACEBOOK);
            if (social != null)
                FacebookId = social.Id;
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