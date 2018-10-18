using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Blog_IT.Models;
using PagedList.Mvc;
using PagedList;
namespace Blog_IT.Controllers
{
    public class TagController : Controller
    {
        BlogITEntities db = new BlogITEntities();
        // GET: Tag
        [Route("Tag/{alias}-{id}/{page?}")]
        public ActionResult Index(int? id, string alias, int? page)
        {
            if(id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
           
            Tag tag = db.Tags.Find(id);
            if(tag == null)
            {
                return RedirectToAction("PageNotFound","StaticContent",new {area = ""});
            }
            ViewBag.TagName = tag.Name;
            var model =  tag.Posts.Where(m => m.Show == true).OrderByDescending(p => p.PostID);
            return View(model.ToPagedList(page ?? 1,10));
        }
        [ChildActionOnly]
        [OutputCache(Duration = 600)]
        public PartialViewResult _ListPartial()
        {
            return PartialView("_ListPartial", db.Tags);
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