// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System.ComponentModel.DataAnnotations;

namespace Foundation.Server.Api
{
    #region guest auth

    /// <summary>
    /// Sign in using the device id as a key. Guest mode.
    /// </summary>
    public class AccountGuestSignIn
    {
        /// <summary>
        /// Unique Id for the user.
        /// </summary>
        [Required]
        public string UserId { get; set; }
    }
    #endregion

    #region email auth
    /// <summary>
    /// Sign In / Sign Up
    /// </summary>
    public class AccountEmailSignIn
    {
        /// <summary>
        /// Unique Id (GUID)
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// User's password
        /// </summary>
        [Required]
        [MinLength(5)]
        public string Password { get; set; }
    }


    /// <summary>
    /// 'I lost my password' request
    /// </summary>
    public class AccountEmailReset
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    /// <summary>
    /// Updates an account's details
    /// </summary>
    public class AccountEmailUpdate
    {
        [EmailAddress]
        public string NewEmail { get; set; }

        public string NewPassword { get; set; }
    }

    /// <summary>
    /// Removes Email claim. will cause account delete.
    /// </summary>
    public class AccountEmailDelete
    {
        /// <summary>
        /// Personal Details (Updated)
        /// </summary>
        [Required]
        public string Password { get; set; }
    }

    #endregion

    #region twilio auth
    /// <summary>
    /// Sign In / Sign Up
    /// </summary>
    public class AccountTwilioSignIn
    {
        public string UserId { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [Required]
        [MinLength(5)]
        public string Password { get; set; }
    }


    /// <summary>
    /// 'I lost my password' request
    /// </summary>
    public class AccountTwilioReset
    {
        [Required]
        [Phone]
        public string Phone { get; set; }
    }

    /// <summary>
    /// Updates an account's details
    /// </summary>
    public class AccountTwilioUpdate
    {
        [Required]
        public string UserId { get; set; }
        
        [Phone]
        [Required]
        public string Phone { get; set; }
        
        [Required]
        public string Password { get; set; }
    }

    /// <summary>
    /// Removes phone claim. will cause account delete.
    /// </summary>
    public class AccountTwilioDelete
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }
        
        [Required]
        [Phone]
        public string Password { get; set; }
    }

    #endregion
    
    #region fb auth
    /// <summary>
    /// Social Sign In
    /// </summary>
    public class AccountFacebookConnect
    {
        /// <summary>
        /// Provider password
        /// </summary>
        [Required]
        public string AccessToken { get; set; }
    }

    /// <summary>
    /// Removes Facebook claim. 
    /// </summary>
    public class AccountFacebookDisconnect
    {

    }

    #endregion

    /// <summary>
    /// Describes the user Account
    /// </summary>
    public class AccountDetails
    {
        /// <summary>
        /// Unique Id (GUID)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// User's Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// User's Phone (Twilio)
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// User's Email
        /// </summary>
        public string FacebookId { get; set; }
    }
}
