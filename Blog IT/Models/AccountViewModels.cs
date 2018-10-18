using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Blog_IT.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessage = "Bạn chưa nhập họ đệm")]
        [StringLength(100, ErrorMessage = "Họ đệm không được vượt quá 100 ký tự.")]
        [Display(Name = "Họ đệm")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Bạn chưa nhập tên")]
        [StringLength(50, ErrorMessage = "Tên không được vượt quá 50 ký tự.")]
        [Display(Name = "Tên")]
        public string LastName { get; set; }

        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [Required(ErrorMessage = "Bạn chưa nhập email")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required(ErrorMessage = "Bạn chưa nhập email")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Bạn chưa nhập email")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Bạn chưa nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Display(Name = "Ghi nhớ?")]
        public bool RememberMe { get; set; }
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Bạn chưa nhập họ đệm")]
        [StringLength(100, ErrorMessage = "Họ đệm không được vượt quá 100 ký tự.")]
        [Display(Name = "Họ đệm")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Bạn chưa nhập tên")]
        [StringLength(50, ErrorMessage = "Tên không được vượt quá 50 ký tự.")]
        [Display(Name = "Tên")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Bạn chưa nhập email")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Bạn chưa nhập mật khẩu")]
        [StringLength(100, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự, tối đa 100 ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Nhập lại mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu và nhập lại mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; }

     
    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Bạn chưa nhập email")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Bạn chưa nhập mật khẩu")]
        [StringLength(100, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự, tối đa 100 ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Nhập lại mật khẩu mới")]
        [Compare("Password", ErrorMessage = "Mật khẩu và nhập lại mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Bạn chưa nhập email")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
