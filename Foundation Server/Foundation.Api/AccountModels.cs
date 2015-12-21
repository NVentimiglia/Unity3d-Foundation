// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------

using System.ComponentModel.DataAnnotations;

namespace Foundation.Server.Api
{
    #region email auth
    /// <summary>
    /// Sign In / Sign Up
    /// </summary>
    public class AccountEmailRequest
    {
        /// <summary>
        /// Unique Id for the user.
        /// This can be App:UserName or just a Guid.
        /// </summary>
        [Required]
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
    public class AccountEmailResetRequest
    {
        /// <summary>
        /// Personal Details (Updated)
        /// </summary>
        [Required]
        public string Token { get; set; }

        /// <summary>
        /// Personal Details (Updated)
        /// </summary>
        [Required]
        public string Password { get; set; }
    }

    /// <summary>
    /// Updates an account's details
    /// </summary>
    public class AccountEmailUpdateRequest
    {
        /// <summary>
        /// Personal Details (Updated)
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Personal Details (Updated)
        /// </summary>
        public string Password { get; set; }
    }

    /// <summary>
    /// Removes Email claim. will cause account delete.
    /// </summary>
    public class AccountEmailDeleteRequest
    {
        /// <summary>
        /// User's unique id
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// Personal Details (Updated)
        /// </summary>
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// Personal Details (Updated)
        /// </summary>
        [Required]
        public string Password { get; set; }
    }

    /// <summary>
    /// Issues a Token as needed for Update
    /// </summary>
    public class AccountRecoverRequest
    {
        /// <summary>
        /// User's email
        /// </summary>
        [Required]
        public string Email { get; set; }
    }
    #endregion
    
    #region guest auth

    /// <summary>
    /// Sign in using the device id as a key. Guest mode.
    /// </summary>
    public class AccountGuestRequest
    {
        /// <summary>
        /// Unique Id for the user.
        /// </summary>
        [Required]
        public string UserId { get; set; }
        }
    

    #endregion

    #region fb auth
    /// <summary>
    /// Social Sign In
    /// </summary>
    public class AccountFacebookRequest
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
    public class AccountFacebookDeleteRequest
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
        /// User's Email
        /// </summary>
        public string FacebookId { get; set; }

        /// <summary>
        /// Serverside's authorization cookie (header token)
        /// </summary>
        public string AuthorizationToken { get; set; }
        /// <summary>
        /// Serverside's session cookie (header token)
        /// </summary>
        public string SessionToken { get; set; }

        /// <summary>
        /// Is Authenticated
        /// </summary>
        public bool IsAuthenticated { get; set; }
    }


    public class AccountConstants
    {
        public static string APPLICATIONID = "APPLICATIONID";
        public static string SESSION = "SESSION";
        public static string AUTHORIZATION = "AUTHORIZATION";
        public static string FACEBOOK = "facebook";
    }
}
