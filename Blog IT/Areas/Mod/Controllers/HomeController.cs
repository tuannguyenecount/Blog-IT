using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Blog_IT.Models;
namespace Blog_IT.Areas.Mod.Controllers
{
    [Authorize(Roles = "Mod")]
    public class HomeController : Controller
    {
        BlogITEntities db = new BlogITEntities();
        public AspNetUser user
        {
            get
            {
                string id = User.Identity.GetUserId();
                return db.AspNetUsers.SingleOrDefault(m => m.Id == id);
            }
            set
            {
                user = value;
            }
        }
        // GET: Mod/Home
        public ActionResult Index()
        {
            ViewBag.Name = user.FullName;
            return View();
        }
    }
}