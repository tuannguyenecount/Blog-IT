using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Blog_IT.Models;
using System.Threading.Tasks;
using PagedList;
using PagedList.Mvc;
using Microsoft.AspNet.Identity;
namespace Blog_IT.Controllers
{
    public class PostController : Controller
    {
        BlogITEntities db = new BlogITEntities();
   
        [ChildActionOnly]
        public ActionResult Featured_ArticlesPartial()
        {
            IEnumerable<Post> featured_Articles = db.Posts.Where(m=>m.Show == true).OrderByDescending(m => m.Views).Take(5);
            return PartialView(featured_Articles);
        }

        [ChildActionOnly]
        public ActionResult Featured_ArticlesPartial2()
        {
            IEnumerable<Post> featured_Articles = db.Posts.Where(m => m.Show == true).OrderByDescending(m => m.Views).Take(5);
            return PartialView(featured_Articles);
        }
        // GET: Post
        [Route("{alias}.html")]
        [OutputCache(Duration = 3600, Location = System.Web.UI.OutputCacheLocation.Server, VaryByParam = "alias")]
        public async Task<ActionResult> Detail(string alias,  string reportMessage, string errorMessage)
        {
            if(alias == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.SingleOrDefault(m => m.Alias == alias);
            if(post == null || post.Show == false)
            {
                return RedirectToAction("PageNotFound", "StaticContent", new { area = "" });
            }
            if (Session["read_post" + post.PostID.ToString()] == null)
            {
                post.Views++;
                db.Entry(post).State = System.Data.Entity.EntityState.Modified;
                await db.SaveChangesAsync();
                Session["read_post" + post.PostID.ToString()] = "readed";
            }
            ViewBag.ReportMessage = reportMessage;
            ViewBag.ErrorMessage = errorMessage;
            ViewBag.RelatedPost = db.Posts.Where(m => m.SubCategoryID == post.SubCategoryID && m.PostID != post.PostID && m.Show == true).Take(5).AsEnumerable();
            return View(post);
        }

        [Authorize]
        [Route("Demo/{alias}.html")]
        public ActionResult Demo(string alias)
        {
            if (alias == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.SingleOrDefault(m => m.Alias == alias);
            if (post == null)
            {
                return RedirectToAction("PageNotFound", "StaticContent", new { area = "" });
            }
            if(User.IsInRole("Admin") == false)
            {
                if(post.UserID != User.Identity.GetUserId())
                {
                    return RedirectToAction("PageNotFound", "StaticContent", new { area = "" });
                }
            }
            ViewBag.RelatedPost = db.Posts.Where(m => m.SubCategoryID == post.SubCategoryID && m.PostID != post.PostID && m.Show == true).Take(5).AsEnumerable();
            return View("Detail",post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddReport(ReportPost reportPost, string alias)
        {
            try
            {
                reportPost.DateReport = DateTime.Now;
                if (ModelState.IsValid)
                {
                    db.ReportPosts.Add(reportPost);
                    await db.SaveChangesAsync();
                    string message = "Báo sai phạm thành công. Xin cảm ơn.";
                    return RedirectToAction("Detail", new { id = reportPost.PostID, alias = alias, reportMessage = message });
                }
                else
                {
                    return RedirectToAction("Detail", new { id = reportPost.PostID, alias = alias, errorMessage = "Xảy ra lỗi khi báo sai phạm bài viết này" });

                }
            }
            catch
            {
                return RedirectToAction("Detail", new { id = reportPost.PostID, alias = alias, errorMessage = "Xảy ra lỗi khi báo sai phạm bài viết này" });

            }
        }

        [Route("category/{alias}")]
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.Server,Duration = 3600, VaryByCustom = "alias", VaryByParam = "page")]
        public ActionResult PostByCategory(string alias, int? page)
        {
            Category category = db.Categories.SingleOrDefault(m => m.Alias == alias);
            if(category == null)
            {
                return RedirectToAction("PageNotFound", "StaticContent");
            }
            ViewBag.CategoryName = category.Name;
            var model = category.Posts.Where(m=>m.Show == true).OrderByDescending(m=>m.PostID);
            return View(model.ToPagedList(page ?? 1, 10));
        }

        [Route("subcategory/{alias}")]
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.Downstream, Duration = 3600)]
        public ActionResult PostBySubCategory(string alias, int? page)
        {
            SubCategory subCategory = db.SubCategories.SingleOrDefault(m => m.Alias == alias);
            if (subCategory == null)
            {
                return RedirectToAction("PageNotFound", "StaticContent");
            }
            ViewBag.SubCategoryName = subCategory.Name;
            var model = subCategory.Posts.Where(m => m.Show == true).OrderByDescending(m => m.PostID);
            return View(model.ToPagedList(page ?? 1, 10));
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