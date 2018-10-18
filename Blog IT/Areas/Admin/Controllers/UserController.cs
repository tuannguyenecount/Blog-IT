using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Blog_IT.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace Blog_IT.Areas.Admin.Controllers
{
    [Authorize(Roles="Admin")]
    public class UserController : Controller
    {
        private BlogITEntities db = new BlogITEntities();


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

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

       
        public UserController()
        {
        }

        public UserController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        public PartialViewResult Search(string searchBy, string searchVal)
        {
            IEnumerable<AspNetUser> Users = null;
            if (searchBy == "FirstName")
                Users = db.AspNetUsers.Where(m => m.FirstName.Contains(searchVal));
            else if(searchBy == "LastName")
            {
                Users = db.AspNetUsers.Where(m => m.LastName.Contains(searchVal));
            }
            else
            {
                Users = db.AspNetUsers.Where(m => m.Email.Contains(searchVal));
            }
            return PartialView(Users);
        }

        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateUserViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user = new ApplicationUser() { FirstName = App_Code.XoaKhoangTrangThua(model.FirstName), LastName = App_Code.XoaKhoangTrangThua(model.LastName), Email = model.Email, UserName = model.Email, Image = "user.png", DateRegister = DateTime.Now, EmailConfirmed = true };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrors(result);
                }
            }
            return View(model);
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
                else if (error.Contains("Passwords must have at least one uppercase ('A'-'Z')."))
                {
                    string customError = "Mật khẩu phải chứa ít nhất 1 ký tự chữ in hoa ('A'-'Z').";
                    ModelState.AddModelError("", customError);
                }
                else if (error.Contains("Login is already taken"))
                {
                    string customError = "Email này đã được được sử dụng.";
                    ModelState.AddModelError("", customError);
                }
                else if (error.Contains("Passwords must have at least one digit ('0'-'9'). Passwords must have at least one uppercase ('A'-'Z')"))
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
        // GET: Admin/User
        public async Task<ActionResult> Index()
        {
            return View(await db.AspNetUsers.ToListAsync());
        }

        // GET: Admin/User/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser aspNetUser = await db.AspNetUsers.Include(m => m.AspNetRoles).Include(m => m.Posts).SingleOrDefaultAsync(m => m.Id == id);
            if (aspNetUser == null)
            {
                return RedirectToAction("PageNotFound", "StaticContent", new { area = "" });
            }
            return View(aspNetUser);
        }

        // GET: Admin/User/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser aspNetUser = await db.AspNetUsers.FindAsync(id);
            if (aspNetUser == null)
            {
                return RedirectToAction("PageNotFound", "StaticContent", new { area = "" });
            }
            return View(aspNetUser);
        }

        // POST: Admin/User/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,FirstName,LastName,PasswordHash,SecurityStamp,AccessFailedCount,Email,EmailConfirmed,PhoneNumber,LockoutEndDateUtc,LockoutEnabled,Image,Introduce,DateRegister")] AspNetUser aspNetUser, HttpPostedFileBase file)
        {
            try
            {
                aspNetUser.PhoneNumberConfirmed = false;
                aspNetUser.TwoFactorEnabled = false;
                aspNetUser.UserName = aspNetUser.Email;
                
                if (file != null && file.ContentLength > 0)
                {
                    string extendFile = System.IO.Path.GetExtension(file.FileName);
                    if (extendFile != ".jpg" && extendFile != ".jpeg" && extendFile != ".png")
                    {
                        ModelState.AddModelError("", "Hình ảnh phải có đuôi .jpg, .jpeg hoặc .png!");
                        return View(aspNetUser);
                    }
                    if (file.ContentLength > 1000141)
                    {
                        ModelState.AddModelError("", "Hình ảnh phải có size < 1MB. Vui lòng cắt bớt hình hoặc chọn hình khác!");
                        return View(aspNetUser);
                    }
                    aspNetUser.Image = aspNetUser.Id.ToString() + extendFile;
                    file.SaveAs(Server.MapPath("~/Photos/Users/" + aspNetUser.Image));
                }
                if (ModelState.IsValid)
                {
                    
                    db.Entry(aspNetUser).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = aspNetUser.Id});
                }
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(aspNetUser);
            }
            return View(aspNetUser);
        }

        // GET: Admin/User/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if(Session[User.Identity.GetUserId()] == null)
            {
                return RedirectToAction("ConfirmationContinue", "Account", new { area = "", Url = Request.Url, UrlReferrer = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : Url.Action("Index", "Home") });
            }
            AspNetUser aspNetUser = await db.AspNetUsers.FindAsync(id);
            if (aspNetUser == null)
            {
                return RedirectToAction("PageNotFound", "StaticContent", new { area = "" });
            }
            return View(aspNetUser);
        }

        // POST: Admin/User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (Session[User.Identity.GetUserId()] == null)
            {
                return RedirectToAction("ConfirmationContinue", "Account", new { area = "", Url = Request.Url, UrlReferrer = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : Url.Action("Index", "Home") });
            }
            AspNetUser aspNetUser = await db.AspNetUsers.FindAsync(id);
            db.AspNetUsers.Remove(aspNetUser);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> CustomRole(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (Session[User.Identity.GetUserId()] == null)
            {
                return RedirectToAction("ConfirmationContinue", "Account", new { area = "", Url = Request.Url, UrlReferrer = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : Url.Action("Index", "Home") });
            }
            AspNetUser aspNetUser = await db.AspNetUsers.Include(m => m.AspNetRoles).SingleOrDefaultAsync(m => m.Id == id);
            if (aspNetUser == null)
            {
                return RedirectToAction("PageNotFound", "StaticContent", new { area = "" });
            }

            ViewBag.lstRole = new List<AspNetRole>();
            foreach(AspNetRole role in db.AspNetRoles)
            {
                if(!aspNetUser.AspNetRoles.Contains(role))
                {
                    ((List<AspNetRole>)(ViewBag.lstRole)).Add(role);
                }
            }
            return View(aspNetUser);
        }
        public ActionResult RemoveRoleFromUser(string userID, string roleID)
        {
            AspNetUser aspNetUser = db.AspNetUsers.Include(m=>m.AspNetRoles).Single(m=>m.Id == userID);
            AspNetRole role = db.AspNetRoles.Find(roleID);
            aspNetUser.AspNetRoles.Remove(role);
            db.SaveChanges();
            return RedirectToAction("CustomRole", new { id = userID });
        }
        public ActionResult AddeRoleToUser(string userID, string roleID)
        {
            AspNetUser aspNetUser = db.AspNetUsers.Include(m => m.AspNetRoles).Single(m => m.Id == userID);
            AspNetRole role = db.AspNetRoles.Find(roleID);
            aspNetUser.AspNetRoles.Add(role);
            db.SaveChanges();
            return RedirectToAction("CustomRole", new { id = userID });
        }
       
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
