using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Blog_IT.Models;
namespace Blog_IT.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        BlogITEntities db = new BlogITEntities();
        // GET: Admin/Home
        public ActionResult Index()
        {
            ViewBag.CountPostNotShow = db.Posts.Count(m => m.Show == false);
            ViewBag.UsersRegisterToDay = db.AspNetUsers.Where(m => m.DateRegister == DateTime.Today).ToList();
            ViewBag.CountNewMailBoxes = db.Mailboxes.Where(m => m.Confirmed == false).Count();
            ViewBag.CountNewReportPosts = db.ReportPosts.Count();
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}