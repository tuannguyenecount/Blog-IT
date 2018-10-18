using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
using Blog_IT.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
namespace Blog_IT.Areas.Admin.Controllers
{
    [Authorize(Roles="Admin")]
    public class ReportPostController : Controller
    {
        private BlogITEntities db = new BlogITEntities();
        // GET: Admin/ReportPost
        public ActionResult Index(int? page)
        {
            IEnumerable<ReportPost> reportsPost = db.ReportPosts.Include(m => m.Post).OrderBy(m=>m.Id);
            return View(reportsPost.ToPagedList(page ?? 1, 5));
        }
        public ActionResult Delete(int? id, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            if (Session[User.Identity.GetUserId()] == null)
            {
                return RedirectToAction("ConfirmationContinue", "Account", new { area = "", Url = Request.Url, UrlReferrer = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : Url.Action("Index", "Home") });
            }
            ReportPost report = db.ReportPosts.Find(id);
            if (report == null)
            {
                return RedirectToAction("PageNotFound", "StaticContent", new { area = "" });
            }
            return View();
        }
        [ActionName("Delete")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public RedirectToRouteResult DeleteConfirmed(int id, int? page)
        {
            if (Session[User.Identity.GetUserId()] == null)
            {
                return RedirectToAction("ConfirmationContinue", "Account", new { area = "", Url = Request.Url, UrlReferrer = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : Url.Action("Index", "Home") });
            }
            ReportPost report = db.ReportPosts.Find(id);
            db.ReportPosts.Remove(report);
            db.SaveChanges();
            return RedirectToAction("Index", new { page = page ?? 1});
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