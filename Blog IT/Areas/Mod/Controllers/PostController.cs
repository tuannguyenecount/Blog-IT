using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Blog_IT.Models;
using PagedList;
using PagedList.Mvc;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
namespace Blog_IT.Areas.Mod.Controllers
{
    [Authorize(Roles = "Mod")]
    public class PostController : Controller
    {
        private BlogITEntities db = new BlogITEntities();
        public string userId
        {
            get
            {
                return User.Identity.GetUserId();
            }
        }

        [ValidateInput(false)]
        [HttpPost]
        public async Task<JsonResult> SavePost(int id, string body)
        {
            try
            {
                Post post = db.Posts.Find(id);
                if(post.UserID != userId)
                {
                    if(User.IsInRole("Admin") == false)
                    {
                        throw new Exception("Bạn không có quyền thao tác với bài viết này!");
                    }
                }
                post.Body = body;
                db.Entry(post).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return Json(new { status = 1, message = "Đã lưu nội dung" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { status = 0, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        // GET: Mod/Post
        public ActionResult Index(int? page)
        {
            var posts = db.Posts.Where(m => m.UserID == userId);
            ViewBag.Title = "Bài viết của tôi";
            ViewBag.Page = page ?? 1;
            return View(posts.OrderBy(m => m.Show).ThenByDescending(m => m.PostID).ToPagedList(page ?? 1, 5));
        }
        [NonAction]
        public IQueryable<Post> SortMethod(IQueryable<Post> posts, string sortname, string sortby)
        {
            if (sortby == "asc")
            {
                if (sortname == "id")
                {
                    posts = posts.OrderBy(m => m.PostID);
                }
                else if (sortname == "view")
                {
                    posts = posts.OrderBy(m => m.Views);
                }
                else if (sortname == "title")
                {
                    posts = posts.OrderBy(m => m.Title);
                }
                else if (sortname == "show")
                {
                    posts = posts.OrderBy(m => m.Show);
                }
            }
            else
            {
                if (sortname == "id")
                {
                    posts = posts.OrderByDescending(m => m.PostID);
                }
                else if (sortname == "view")
                {
                    posts = posts.OrderByDescending(m => m.Views);
                }
                else if (sortname == "title")
                {
                    posts = posts.OrderByDescending(m => m.Title);
                }
                else if (sortname == "show")
                {
                    posts = posts.OrderByDescending(m => m.Show);
                }
            }
            return posts;
        }

        public PartialViewResult Sort(string sortname, string sortby)
        {
            var posts = db.Posts.Where(m=>m.UserID == userId);
            posts = SortMethod(posts, sortname, sortby);
            if (sortby == "asc")
            {
                ViewBag.sortNext = "desc";
            }
            else
            {
                ViewBag.sortNext = "asc";
            }
            ViewBag.sortName = sortname;
            ViewBag.sortBy = sortby;
            return PartialView("_SortPartial", posts.ToPagedList(1, 5));
        }
        public PartialViewResult Search(string keyword)
        {
            var posts = db.Posts.Where(m => m.UserID == userId && m.Title.Contains(keyword));
            return PartialView("_SearchPartial", posts.AsEnumerable());
        }

        public PartialViewResult Pager(int page, string sortname, string sortby)
        {
            var posts = db.Posts.Where(m=>m.UserID == userId);
            posts = SortMethod(posts, sortname, sortby);
            ViewBag.sortName = sortname;
            ViewBag.sortBy = sortby;
            ViewBag.Page = page;
            return PartialView("_PagerPartial", posts.ToPagedList(page, 5));
        }
        // GET: Mod/Post/Details/5
        public ActionResult Details(int? id, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.SingleOrDefault(m => m.PostID == id && m.UserID == userId);
            if (post == null)
            {
                return RedirectToAction("PageNotFound", "StaticContent", new { area = "" });
            }
            ViewBag.Page = page ?? 1;
            return View(post);
        }

        public JsonResult SelectSubCategory(string CategoryID)
        {
            return Json(db.SubCategories.Where(m => m.CategoryID == CategoryID).Select(m => new { ID = m.ID, Name = m.Name }).ToArray(), JsonRequestBehavior.AllowGet);
        }
        // GET: Mod/Post/Create
        public async Task<ActionResult> Create()
        {
            Post newPost = new Post()
            {
                Title = "Bản nháp bài viết BaiViet_Ticks" + DateTime.Now.Ticks.ToString(),
                Show = false,
                CategoryID = db.Categories.First().ID
            };
            SubCategory subCategory = db.SubCategories.FirstOrDefault(m => m.CategoryID == newPost.CategoryID);
            if (subCategory != null)
            {
                newPost.SubCategoryID = subCategory.ID;
            }
            newPost.Image = "nhap.png";
            newPost.UserID = User.Identity.GetUserId();
            newPost.Views = 0;
            newPost.Introduce = "...";
            newPost.Body = "...";
            newPost.DatePost = DateTime.Now;
            newPost.Alias = App_Code.VietnameseSigns(newPost.Title);
            db.Posts.Add(newPost);
            await db.SaveChangesAsync();
            newPost.Title = "Bài viết số " + newPost.PostID.ToString();
            db.Entry(newPost).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return RedirectToAction("Edit", new { id = newPost.PostID, create = true });
        }

      
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[ValidateInput(false)]
        //public async Task<ActionResult> Create([Bind(Include = "PostID,Title,Introduce,Body,CategoryID, SubCategoryID")] Post post, HttpPostedFileBase file, int[] TagID)
        //{
        //    try
        //    {
        //        if (db.Posts.FirstOrDefault(m => m.Title.ToLower().Trim() == post.Title.ToLower().Trim()) != null)
        //        {
        //            ModelState.AddModelError("", "Tiêu đề đã bị trùng bài viết khác. Bạn hãy đổi tiêu đề cho bài viết.");
        //        }
        //        post.Alias = App_Code.VietnameseSigns(post.Title.ToLower());
        //        post.DatePost = DateTime.Today;
        //        if (User.IsInRole("Admin") == false)
        //            post.Show = false;
        //        else
        //            post.Show = true;
        //        post.Views = 0;
        //        post.UserID = userId;
        //        if (file != null && file.ContentLength > 0)
        //        {
        //            string extendFile = System.IO.Path.GetExtension(file.FileName);
        //            if (extendFile != ".jpg" && extendFile != ".jpeg" && extendFile != ".png")
        //            {
        //                ModelState.AddModelError("", "Hình ảnh phải có đuôi .jpg, .jpeg hoặc .png!");
        //                ViewBag.TagID = new SelectList(db.Tags, "TagID", "Name");
        //                return View(post);
        //            }
        //            if (file.ContentLength > 1000141)
        //            {
        //                ModelState.AddModelError("", "Hình ảnh phải có size < 1MB. Vui lòng cắt bớt hình hoặc chọn hình khác!");
        //                ViewBag.TagID = new SelectList(db.Tags, "TagID", "Name");
        //                return View(post);
        //            }
        //            post.Image = post.Title + extendFile;
        //            file.SaveAs(Server.MapPath("~/Photos/Posts/" + post.Image));
        //            System.Drawing.Image image = System.Drawing.Image.FromFile(Server.MapPath("~/Photos/Posts/") + post.Image);
        //            using (var resized = ImageUtilities.ResizeImage(image, 64, 54))
        //            {
        //                //save the resized image as a jpeg with a quality of 100'
        //                ImageUtilities.SaveJpeg(Server.MapPath("~/Photos/ImageSmall/") + post.Image, resized, 100);
        //            }
        //        }
        //        if (ModelState.IsValid)
        //        {
        //            db.Posts.Add(post);
        //            if (TagID != null && TagID.Count() > 0)
        //            {
        //                post.Tags = new List<Tag>();
        //                foreach (int tagID in TagID)
        //                {
        //                    Tag tag = db.Tags.Find(tagID);

        //                    post.Tags.Add(tag);
        //                }
        //            }
        //            await db.SaveChangesAsync();
                
        //            return RedirectToAction("Index");
        //        }
        //        ViewBag.TagID = new SelectList(db.Tags, "TagID", "Name");
        //        ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name", post.CategoryID);
        //        ViewBag.SubCategoryID = new SelectList(db.SubCategories.Where(m => m.CategoryID == post.CategoryID), "ID", "Name");
        //        return View(post);
        //    }
        //    catch
        //    {
        //        ModelState.AddModelError("", "Xảy ra lỗi khi xử lý.");
        //        ViewBag.TagID = new SelectList(db.Tags, "TagID", "Name");
        //        ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name", post.CategoryID);
        //        ViewBag.SubCategoryID = new SelectList(db.SubCategories.Where(m => m.CategoryID == post.CategoryID), "ID", "Name");
        //        return View(post);
        //    }
        //}

        // GET: Mod/Post/Edit/5
        public ActionResult Edit(int? id, int? page, bool? create)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.SingleOrDefault(m => m.PostID == id && m.UserID == userId);
            if (post == null)
            {
                return RedirectToAction("PageNotFound", "StaticContent", new { area = "" });
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name", post.CategoryID);
            if (post.Category.SubCategories.Count > 0)
            {
                ViewBag.SubCategoryID = new SelectList(db.SubCategories.Where(m => m.CategoryID == post.CategoryID), "ID", "Name", post.SubCategoryID);
            }
            else
            {
                ViewBag.SubCategoryID = new SelectList(new List<SubCategory>(), "ID", "Name");

            }
            ViewBag.Page = page ?? 1;
            ViewBag.Message = create == null ? "Sửa bài viết số " + post.PostID.ToString() : "Tạo bài viết số " + post.PostID.ToString();

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Edit([Bind(Include = "PostID, Title, Introduce, Body, CategoryID, SubCategoryID, Views, Image, ImageOpenGraph, DatePost, DateModified")] Post post, int? page, HttpPostedFileBase file, HttpPostedFileBase fileOpenGraph, string titleOld)
        {
            try
            {
                if (titleOld.ToLower().Trim() != post.Title.ToLower().Trim())
                {
                    if (db.Posts.FirstOrDefault(m => m.Title.ToLower().Trim() == post.Title.ToLower().Trim() && m.Show == true) != null)
                    {
                        ModelState.AddModelError("", "Tiêu đề đã bị trùng bài viết khác. Bạn hãy đổi tiêu đề cho bài viết.");
                    }
                }
                post.Alias = App_Code.VietnameseSigns(post.Title.ToLower());
                post.UserID = userId;
                post.DateModified = DateTime.Now;
                if (User.IsInRole("Admin") == false)
                    post.Show = false;
                else
                    post.Show = true;

                if (file != null && file.ContentLength > 0)
                {
                    string extendFile = System.IO.Path.GetExtension(file.FileName);
                    if (extendFile != ".jpg" && extendFile != ".jpeg" && extendFile != ".png")
                    {
                        ModelState.AddModelError("", "Hình ảnh phải có đuôi .jpg, .jpeg hoặc .png!");
                        return View(post);
                    }
                    if (file.ContentLength > 1000141)
                    {
                        ModelState.AddModelError("", "Hình ảnh phải có size < 1MB. Vui lòng cắt bớt hình hoặc chọn hình khác!");
                        return View(post);
                    }
                    post.Image = post.Alias + extendFile;
                    file.SaveAs(Server.MapPath("~/Photos/Posts/" + post.Image));
                    System.Drawing.Image image = System.Drawing.Image.FromFile(Server.MapPath("~/Photos/Posts/") + post.Image);
                    using (var resized = ImageUtilities.ResizeImage(image, 64, 54))
                    {
                        ImageUtilities.SaveJpeg(Server.MapPath("~/Photos/ImageSmall/") + post.Image, resized, 100);
                    }
                }
                if (fileOpenGraph != null && fileOpenGraph.ContentLength > 0)
                {
                    string extendFile = System.IO.Path.GetExtension(fileOpenGraph.FileName);
                    if (extendFile != ".jpg" && extendFile != ".jpeg" && extendFile != ".png")
                    {
                        ModelState.AddModelError("", "Hình ảnh open graph phải có đuôi .jpg, .jpeg hoặc .png!");
                    }
                    post.ImageOpenGraph = post.Alias + extendFile;
                    fileOpenGraph.SaveAs(Server.MapPath("~/Photos/ImageOpenGraph/" + post.ImageOpenGraph));
                }
                if (ModelState.IsValid)
                {
                    db.Entry(post).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return RedirectToAction("Edit", new { id = post.PostID, page = page });
                }
                ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name", post.CategoryID);
                ViewBag.SubCategoryID = new SelectList(db.SubCategories.Where(m => m.CategoryID == post.CategoryID), "ID", "Name", post.SubCategoryID);
                return View(post);
            }
            catch
            {
                ModelState.AddModelError("", "Xảy ra lỗi khi xử lý.");
                ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name", post.CategoryID);
                ViewBag.SubCategoryID = new SelectList(db.SubCategories.Where(m => m.CategoryID == post.CategoryID), "ID", "Name", post.SubCategoryID);
                return View(post);
            }
        }

        // GET: Mod/Post/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            if (Session[userId] == null)
            {
                return RedirectToAction("ConfirmationContinue", "Account", new { area = "", Url = Request.Url, UrlReferrer = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : Url.Action("Index", "Home") });
            }
            Post post = await db.Posts.FindAsync(id);
            if (post == null || post.UserID != userId)
            {
                return RedirectToAction("PageNotFound", "StaticContent", new { area = "" });
            }

            return View(post);
        }

        // POST: Mod/Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Post post = null;
            try
            {
                post = db.Posts.Find(id);
                db.Posts.Remove(post);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "Xảy ra lỗi khi xử lý");
                return View("Delete", post);
            }
        }

        public ActionResult TagsOfPost(int? id, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            Post post = db.Posts.Find(id);
            if (post == null)
            {
                return RedirectToAction("PageNotFound", "StaticContent", new { area = "" });
            }
            ViewBag.TitleArticle = post.Title;
            ViewBag.PostID = post.PostID;
            var tags = post.Tags;
            ViewBag.Page = page ?? 1;
            return View(tags);
        }
        public async Task<ActionResult> AddTags(int? id, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            Post post = await db.Posts.FindAsync(id);
            if (post == null)
            {
                return RedirectToAction("PageNotFound", "StaticContent", new { area = "" });
            }
            var tagIDs = post.Tags.Select(m => m.TagID);
            ViewBag.TagID = new SelectList(db.Tags.Where(m => tagIDs.Contains(m.TagID) == false), "TagID", "Name");
            ViewBag.PostID = post.PostID;
            ViewBag.TitleArticle = post.Title;
            ViewBag.Page = page ?? 1;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddTags(List<int> TagID, int postID, int page)
        {
            try
            {
                Post post = db.Posts.Find(postID);
                foreach (int tagID in TagID)
                {
                    Tag tag = db.Tags.Find(tagID);
                    post.Tags.Add(tag);
                }
                await db.SaveChangesAsync();
                return RedirectToAction("TagsOfPost", new { id = postID, page = page });
            }
            catch
            {
                ModelState.AddModelError("", "Xảy ra lỗi khi xử lý");
                Post post = db.Posts.Find(postID);
                var tagIDs = post.Tags.Select(m => m.TagID);
                ViewBag.TagID = new SelectList(db.Tags.Where(m => tagIDs.Contains(m.TagID) == false), "TagID", "Name");
                ViewBag.PostID = post.PostID;
                ViewBag.TitleArticle = post.Title;
                ViewBag.Page = page;
                return View();
            }
        }
        public ActionResult DeleteTag(int tagID, int postID, int? page)
        {
            Post post = db.Posts.Find(postID);
            Tag tag = db.Tags.Find(tagID);
            post.Tags.Remove(tag);
            db.SaveChanges();
            return RedirectToAction("TagsOfPost", new { id = postID, page = page ?? 1 });
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
