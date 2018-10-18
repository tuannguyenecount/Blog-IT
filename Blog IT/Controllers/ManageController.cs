using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Blog_IT.Models;

namespace Blog_IT.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        BlogITEntities db = new BlogITEntities();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        
        public  string userId
        {
            get
            {
                return User.Identity.GetUserId();
            }
        }
        public AspNetUser user
        {
            get
            {
                return db.AspNetUsers.Find(User.Identity.GetUserId());
            }
            set
            {
                user = value;
            }
        }
        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {

            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Đổi mật khẩu thành công."
                : message == ManageMessageId.SetPasswordSuccess ? "Đã tạo mật khẩu thành công."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "Xảy ra lỗi khi thực hiện thao tác."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : message == ManageMessageId.ChangeImageSuccess ? "Thay hình đại diện thành công. Tải lại trang nếu chưa thấy hình đại diện mới."
                : message == ManageMessageId.ChangeProfileSuccess ? "Sửa thông tin thành công" :  "";

           
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            ViewBag.User = user;
            return View(model);
        }

        public ActionResult EditProfile()
        {
            AspNetUser user = db.AspNetUsers.Find(userId);
            return View(user);
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> EditProfile(AspNetUser model)
        {
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.Introduce = model.Introduce;
            
            if(ModelState.IsValid)
            {
                db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { message = ManageMessageId.ChangeProfileSuccess });
            }
            return View(model);
        }

        public ViewResult ChangePhoto()
        {
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePhoto(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                try
                {
                    string extendFile = System.IO.Path.GetExtension(file.FileName);
                    if(extendFile != ".jpg" && extendFile != ".jpeg" && extendFile != ".png")
                    {
                        ModelState.AddModelError("customError", "Hình ảnh phải có đuôi .jpg, .jpeg hoặc .png!");
                        return View(user);
                    }
                    if(file.ContentLength > 1000141)
                    {
                        ModelState.AddModelError("customError", "Hình ảnh phải có size < 1MB. Vui lòng cắt bớt hình hoặc chọn hình khác!");
                        return View(user);
                    }
                    user.Image = user.Id + extendFile; 
                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    await db.SaveChangesAsync();
                    file.SaveAs(Server.MapPath("~/Photos/Users/" + user.Image));
                }
                catch
                {
                    return RedirectToAction("Index", new { message = ManageMessageId.Error });
                }
            }
            return RedirectToAction("Index", new { message = ManageMessageId.ChangeImageSuccess});
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }



      

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

#region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                if (error.Contains("Incorrect password"))
                {
                    ModelState.AddModelError("", "Mật khẩu không đúng.");
                }
                else if (error.Contains("Passwords must have at least one digit ('0'-'9'). Passwords must have at least one uppercase ('A'-'Z')"))
                {
                    string customError = "Mật khẩu phải chứa ít nhất 1 ký tự số ('0'-'9').Mật khẩu phải chứa ít nhất 1 ký tự chữ in hoa ('A'-'Z').";
                    ModelState.AddModelError("", customError);
                }
                else
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(userId);
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(userId);
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            ChangeImageSuccess,
            ChangeProfileSuccess,
            Error
        }

#endregion
    }
}