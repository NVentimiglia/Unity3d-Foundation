namespace Foundation.Server.Models
{
    /// <summary>
    /// User Recovery / Update confirmation token
    /// </summary>
    public class UserTokenViewModel : UserEmailViewModel
    {
        public string Token { get; set; }
    }

    /// <summary>
    /// User Email Model
    /// </summary>
    public class UserUpdateViewModel : UserEmailViewModel
    {
        public string OldEmail { get; set; }
    }

    /// <summary>
    /// User Email Model
    /// </summary>
    public class UserEmailViewModel
    {
        public string UserId { get; set; }
        public string UserEmail { get; set; }
    }
}