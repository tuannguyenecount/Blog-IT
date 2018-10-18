using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace Blog_IT.Models
{
    [MetadataType(typeof(TagMetadata))]
    public partial class Tag
    {

    }
    sealed class TagMetadata
    {
        [Display(Name = "ID")]
        public int TagID { get; set; }
        [Display(Name = "Tên")]
        [Required(ErrorMessage = "Bạn chưa nhập tên của tag.")]
        public string Name { get; set; }
        [Display(Name = "Bí danh")]
        public string Alias { get; set; }
    }

    [MetadataType(typeof(ReportPostMetadata))]
    public partial class ReportPost
    {

    }
    sealed class ReportPostMetadata
    {
        public int Id { get; set; }
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ. Xin kiểm tra lại.")]
        [Required(ErrorMessage = "Bạn chưa nhập email.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Bạn chưa nhập nội dung")]
        [MinLength(10,ErrorMessage = "Nội dung tối thiểu 10 ký tự")]
        public string Content { get; set; }
        [Display(Name = "Bài viết bị báo sai phạm")]
        [Required]
        public int PostID { get; set; }
        [Display(Name = "Ngày báo sai phạm")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}")]
        public System.DateTime DateReport { get; set; }
    }

    [MetadataType(typeof(AspNetUserMetadata))]
    public partial class AspNetUser
    {
        [Display(Name = "Họ tên")]
        public string FullName
        {
            get
            {
                return FirstName.Trim() + " " + LastName.Trim();
            }
        }
    }
    sealed class AspNetUserMetadata
    {
        public string Id { get; set; }
        [Display(Name = "Họ đệm")]
        [StringLength(100, ErrorMessage = "Họ đệm không được vượt quá 100 ký tự.")]
        [Required(ErrorMessage = "Họ đệm không được bỏ trống.")]
        public string FirstName { get; set; }
        [Display(Name = "Tên")]
        [StringLength(100, ErrorMessage = "Tên không được vượt quá 50 ký tự.")]
        [Required(ErrorMessage = "Tên không được bỏ trống.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email không được bỏ trống.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        public string Email { get; set; }
        [Display(Name = "Xác thực Email")]
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        [Display(Name = "SĐT")]
        [RegularExpression(@"^(0\d{9,12})$", ErrorMessage = "Số điện thoại không hợp lệ.")]
        [DisplayFormat(NullDisplayText = "Chưa có")]
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        [Display(Name = "Khóa đến ngày:")]
        public Nullable<System.DateTime> LockoutEndDateUtc { get; set; }
        [Display(Name = "Kịch hoạt khóa")]
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string UserName { get; set; }
        [Display(Name = "Hình đại diện")]
        public string Image { get; set; }
        [Display(Name = "Giới thiệu")]
        [DisplayFormat(NullDisplayText = "Chưa có giới thiệu bản thân.")]
        public string Introduce { get; set; }
        [Display(Name = "Ngày đăng ký")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public System.DateTime DateRegister { get; set; }

      
    }
  
    [MetadataType(typeof(MailboxMetadata))]
    public partial class Mailbox
    {
      
    }
    sealed class MailboxMetadata
    {
        
        public int ID { get; set; }
        [Display(Name = "Tên")]
        [Required(ErrorMessage  = "Bạn chưa nhập tên.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Bạn chưa nhập địa chỉ email.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        public string Email { get; set; }
        [Display(Name = "Nội dung")]
        [Required(ErrorMessage = "Bạn chưa nhập nội dung.")]
        public string Message { get; set; }
        [Display(Name = "Ngày gửi")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss tt}")]
        public System.DateTime SendDate { get; set; }
        [Display(Name = "Đã xem")]
        public bool Confirmed { get; set; }

    }
    [MetadataType(typeof(PostMetadata))]
    public partial class Post
    {

    }
    sealed class PostMetadata
    {
        [Display(Name = "ID")]
        public int PostID { get; set; }
        [Display(Name = "Chủ đề")]
        public string CategoryID { get; set; }
        [Display(Name = "Chủ đề con")]
        public string SubCategoryID { get; set; }
        [Display(Name = "Tiêu đề")]
        [StringLength(200,ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự.")]
        [Required(ErrorMessage = "Tiêu đề không được bỏ trống.")]
        public string Title { get; set; }
        [Display(Name = "Giới thiệu")]
        [Required(ErrorMessage = "Giới thiệu bài viết không được bỏ trống.")]
        public string Introduce { get; set; }
        [Display(Name = "Nội dung")]
        [Required(ErrorMessage = "Nội dung không được bỏ trống.")]
        public string Body { get; set; }
        [Display(Name = "Hình bìa")]
        public string Image { get; set; }
        [Display(Name = "Bí danh")]
        public string Alias { get; set; }
        [Display(Name = "Lượt xem")]
        public int Views { get; set; }
        [Display(Name = "Tác giả")]
        public string UserID { get; set; }
        [Display(Name = "Ngày đăng")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [DataType(DataType.DateTime)]
        public System.DateTime DatePost { get; set; }
        [Display(Name = "Hiển thị")]
        public bool Show { get; set; }
        [Display(Name = "Hình Open Graph")]
        public string ImageOpenGraph { get; set; }
        [Display(Name = "Ngày sửa cuối")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", NullDisplayText = "Chưa có lần sửa")]
        public Nullable<System.DateTime> DateModified { get; set; }
    }
}