using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
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

    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        BlogITEntities db = new BlogITEntities();
        
        public string userId
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
        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
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
        [Authorize]
        public ViewResult ConfirmationContinue(string Url, string UrlReferrer)
        {
            ViewBag.UrlReferrer = UrlReferrer;
            ViewBag.Url = Url;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmationContinue(string Password, string Url, string UrlReferrer)
        {
            if(Password == string.Empty)
            {
                ModelState.AddModelError("", "Bạn chưa nhập mật khẩu.");
                ViewBag.UrlReferrer = UrlReferrer;
                ViewBag.Url = Url;
                return View();
            }
            
            var result = SignInManager.PasswordSignIn(user.Email, Password, false, shouldLockout: false);
            if(result == SignInStatus.Success)
            {
                Session[user.Id] = true;
                return Redirect(Url);
            }
            else
            {
                ModelState.AddModelError("", "Mật khẩu không đúng.");
                ViewBag.UrlReferrer = UrlReferrer;
                ViewBag.Url = Url;
                return View();
            }
        }
        //
        // GET: /Account/Login
        [AllowAnonymous]
        [Route("dang-nhap")]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if(User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage", new { area = "" });
            }
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("dang-nhap")]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            AspNetUser user = db.AspNetUsers.FirstOrDefault(m => m.Email == model.Email);

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    {
                        HttpContext.Session.Timeout = 120;
                        return RedirectToAction("Index", "Manage", new { area = "" });
                    }
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Email hoặc mật khẩu sai.");
                    return View(model);
            }
        }


        //
        // GET: /Account/Register
        //[AllowAnonymous]
        //public ActionResult Register()
        //{
        //    return View();
        //}

        ////
        //// POST: /Account/Register
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Register(RegisterViewModel model)
        //{
          
        //    if (ModelState.IsValid)
        //    {
        //        if (HttpContext.Session["captchastring"] != null)
        //        {
        //            if (model.CaptchaText != HttpContext.Session["captchastring"].ToString())
        //            {
        //                ModelState.AddModelError("", "Mã xác nhận không đúng.");
        //                return View(model);
        //            }
        //        }
        //        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FirstName = App_Code.XoaKhoangTrangThua(model.FirstName), LastName = App_Code.XoaKhoangTrangThua(model.LastName), Image = "user.png", DateRegister = DateTime.Now };
        //        var result = await UserManager.CreateAsync(user, model.Password);
        //        if (result.Succeeded)
        //        {
        //            await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);
                    
        //            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
        //            // Send an email with this link
        //            string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
        //            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
        //            await UserManager.SendEmailAsync(user.Id, "Xác thực tài khoản website blogit.net", "Vui lòng xác thực tài khoản của bạn bằng cách click vào <a href=\"" + callbackUrl + "\">đây</a>");

        //            ViewBag.Message = "Chúng tôi đã gửi 1 mã xác thực đến email của bạn. Vui lòng kiểm tra email của bạn để thực hiện xác thực tài khoản và hoàn tất đăng ký.";

        //            return View(model);
        //        }
        //        AddErrors(result);
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(model);
        //}

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Khôi phục mật khẩu", "Để thực hiện khôi phục mật khẩu bạn hãy click vào <a href=\"" + callbackUrl + "\">đây</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string userID, string code)
        {
            
            AspNetUser user = db.AspNetUsers.Find(userID);
            if(user == null || code == null)
            {
                return View("Error");
            }
            else
            {
                ViewBag.Email = user.Email;
                return View();
            }
            //return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
            
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
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
                if (error.Contains("Passwords must have at least one digit ('0'-'9')"))
                {
                    string customError = "Mật khẩu phải chứa ít nhất 1 ký tự số ('0'-'9').";
                    ModelState.AddModelError("", customError);
                }
                else if(error.Contains("Passwords must have at least one uppercase ('A'-'Z')."))
                {
                    string customError = "Mật khẩu phải chứa ít nhất 1 ký tự chữ in hoa ('A'-'Z').";
                    ModelState.AddModelError("", customError);
                }
                else if(error.Contains("Login is already taken"))
                {
                    string customError = "Email này đã được được sử dụng.";
                    ModelState.AddModelError("", customError);
                }
                else if(error.Contains("Passwords must have at least one digit ('0'-'9'). Passwords must have at least one uppercase ('A'-'Z')"))
                {
                    string customError = "Mật khẩu phải chứa ít nhất 1 ký tự số ('0'-'9').Mật khẩu phải chứa ít nhất 1 ký tự chữ in hoa ('A'-'Z').";
                    ModelState.AddModelError("", customError);
                }
                else
                {
                    ModelState.AddModelError("", error);
                }
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}