using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SmartDb4.Attributes;
using SmartDb4.Helpers;

namespace SmartDb4.Models
{
    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }

    public class LocalPasswordModel
    {
        //[Required(ErrorMessage = ErrorMessages.RequiredString)]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = ErrorMessages.RequiredString)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string UserName { get; set; }
    }

    public class LoginModel
    {
        //[Required(ErrorMessage = ErrorMessages.RequiredString)]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [DataType(DataType.EmailAddress)]
        public string UserName { get; set; }

        [Required(ErrorMessage = ErrorMessages.RequiredString)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required(ErrorMessage = ErrorMessages.RequiredString)]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = ErrorMessages.RequiredString)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }

    public class LostPasswordModel
    {
        [Required(ErrorMessage = "We need your email to send you a reset link!")]
        [Display(Name = "Your account email")]
        [EmailAddress(ErrorMessage = "Not a valid email--what are you trying to do here?")]
        public string Email { get; set; }
    }

    public class ResetPasswordModel
    {
        [Required]
        [Display(Name = "New Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "New password and confirmation does not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string ReturnToken { get; set; }
    }
}
