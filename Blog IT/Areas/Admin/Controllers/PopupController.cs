using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Blog_IT.Models;
namespace Blog_IT.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PopupController : Controller
    {
        BlogITEntities db = new Models.BlogITEntities();
        // GET: Admin/Popup
        public ActionResult Index()
        {
           
            if (db.Popups.FirstOrDefault() == null)
                return HttpNotFound();
            return View(db.Popups.First());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Index(Popup popup)
        {
            if(ModelState.IsValid)
            {
                db.Entry(popup).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            return View(popup);
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