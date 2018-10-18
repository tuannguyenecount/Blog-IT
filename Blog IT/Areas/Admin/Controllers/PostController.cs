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
using System.Globalization;
namespace Blog_IT.Areas.Admin.Controllers
{
    [Authorize(Roles="Admin")]
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

        //// GET: Admin/Post
        public ActionResult Index(int? page)
        {
            var posts = db.Posts;
            ViewBag.Title = "Danh sách bài viết";
            ViewBag.Page = page ?? 1;
            ViewBag.UserID = db.AspNetUsers.Where(m=>m.AspNetRoles.Count > 0).AsEnumerable();
            return View(posts.OrderBy(m=>m.Show).ThenByDescending(m=>m.PostID).ToPagedList(page ?? 1, 5));
        }

        [ValidateInput(false)]
        [HttpPost]
        public async  Task<JsonResult> SavePost(int id, string body)
        {
            try
            {
                Post post = db.Posts.Find(id);
                post.Body = body;
                db.Entry(post).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return Json(new { status = 1, message = "Đã lưu nội dung" }, JsonRequestBehavior.AllowGet);
                
            }
            catch(Exception ex)
            {
                return Json(new { status = 0, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            
        }

        [NonAction]
        public IQueryable<Post> SortMethod (IQueryable<Post> posts, string sortname, string sortby)
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
            var posts = db.Posts.AsQueryable();
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
            var posts = db.Posts.Where(m => m.Title.Contains(keyword));
            return PartialView("_SearchPartial", posts.AsEnumerable());
        }

        public PartialViewResult Pager(int page, string sortname, string sortby)
        {
            var posts = db.Posts.AsQueryable();
            posts = SortMethod(posts, sortname, sortby);
            ViewBag.sortName = sortname;
            ViewBag.sortBy = sortby;
            ViewBag.Page = page;
            return PartialView("_PagerPartial", posts.ToPagedList(page,5));
        }

        public ActionResult FilterByUser(string UserID)
        {
            var posts = db.Posts.Where(m => m.UserID == UserID);
            ViewBag.FullName = db.AspNetUsers.Find(UserID).FullName;
            return View(posts.OrderBy(m=>m.PostID).AsEnumerable());
        }

        // GET: Admin/Post/Details/5
        public ActionResult Details(int? id, int? page)
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
            ViewBag.Page = page ?? 1;
            return View(post);
        }

        public JsonResult SelectSubCategory(string CategoryID)
        {
            return Json(db.SubCategories.Where(m => m.CategoryID == CategoryID).Select(m=> new { ID = m.ID, Name = m.Name} ).ToArray(), JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/Post/Create
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

        //// POST: Admin/Post/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[ValidateInput(false)]
        //public async Task<ActionResult> Create([Bind(Include = "PostID, Title, UserID, Introduce, CategoryID, SubCategoryID, Body, Show, Views")] Post post, HttpPostedFileBase file, int[] TagID)
        //{
        //    try
        //    {
                
        //        if (db.Posts.FirstOrDefault(m => m.Title.Trim() == post.Title.Trim()) != null)
        //        {
        //            ModelState.AddModelError("", "Tiêu đề đã bị trùng bài viết khác. Bạn hãy đổi tiêu đề cho bài viết.");
        //        }
        //        if(post.Views < 0)
        //        {
        //            ModelState.AddModelError("", "Lượt xem phải >= 0");
        //        }
        //        post.Alias = App_Code.VietnameseSigns(post.Title.ToLower());
        //        post.DatePost = DateTime.Today;
        //        if (file != null && file.ContentLength > 0)
        //        {
        //            string extendFile = System.IO.Path.GetExtension(file.FileName);
        //            if (extendFile != ".jpg" && extendFile != ".jpeg" && extendFile != ".png")
        //            {
        //                ModelState.AddModelError("", "Hình ảnh phải có đuôi .jpg, .jpeg hoặc .png!");
        //            }
        //            if (file.ContentLength > 1000141)
        //            {
        //                ModelState.AddModelError("", "Hình ảnh phải có size < 1MB. Vui lòng cắt bớt hình hoặc chọn hình khác!");
        //            }
        //            post.Image = post.Alias + extendFile;
        //            file.SaveAs(Server.MapPath("~/Photos/Posts/" + post.Image));
        //            System.Drawing.Image image = System.Drawing.Image.FromFile(Server.MapPath("~/Photos/Posts/") + post.Image);
        //            using (var resized = ImageUtilities.ResizeImage(image, 64, 54))
        //            {
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
        //        ViewBag.UserID = new SelectList(db.AspNetUsers.Where(m => m.AspNetRoles.Count > 0), "Id", "Email", post.UserID);
        //        ViewBag.TagID = new SelectList(db.Tags, "TagID", "Name");
        //        ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name", post.CategoryID);
        //        ViewBag.SubCategoryID = new SelectList(db.SubCategories.Where(m => m.CategoryID == post.CategoryID), "ID", "Name");
        //        return View(post);
        //    }
        //    catch
        //    {
        //        ModelState.AddModelError("", "Xảy ra lỗi khi xử lý");
        //        ViewBag.UserID = new SelectList(db.AspNetUsers.Where(m => m.AspNetRoles.Count > 0), "Id", "Email", post.UserID);
        //        ViewBag.TagID = new SelectList(db.Tags, "TagID", "Name");
        //        ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name", post.CategoryID);
        //        ViewBag.SubCategoryID = new SelectList(db.SubCategories.Where(m => m.CategoryID == post.CategoryID), "ID", "Name");
        //        return View(post);
        //    }
        //}

        // GET: Admin/Post/Edit/5

        public async  Task<ActionResult> Edit(int? id, bool? create, int? page)
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
           
            ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name", post.CategoryID);
            ViewBag.UserID = new SelectList(db.AspNetUsers.Where(m => m.AspNetRoles.Count > 0), "Id", "Email", post.UserID);
            if (post.Category.SubCategories.Count > 0)
            {
                ViewBag.SubCategoryID = new SelectList(db.SubCategories.Where(m => m.CategoryID == post.CategoryID), "ID", "Name", post.SubCategoryID);
            }
            else
            {
                ViewBag.SubCategoryID = new SelectList(new List<SubCategory>(), "ID", "Name");

            }
            ViewBag.Message = create == null ? "Sửa bài viết số " + post.PostID.ToString() : "Tạo bài viết số " + post.PostID.ToString(); 
            return View(post);
        }


        // POST: Admin/Post/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Edit([Bind(Include = "PostID, Title, Introduce, UserID, CategoryID, SubCategoryID, Body, Image, ImageOpenGraph, Views,  Show, Views, DatePost, DateModified")] Post post, int? page, HttpPostedFileBase file, HttpPostedFileBase fileOpenGraph, string titleOld, bool? ShowOld)
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

                if (post.Views < 0)
                {
                    ModelState.AddModelError("", "Lượt xem phải >= 0");
                }
                post.Alias = App_Code.VietnameseSigns(post.Title.ToLower());
                post.DateModified = DateTime.Now;

                if (file != null && file.ContentLength > 0)
                {
                    string extendFile = System.IO.Path.GetExtension(file.FileName);
                    if (extendFile != ".jpg" && extendFile != ".jpeg" && extendFile != ".png")
                    {
                        ModelState.AddModelError("", "Hình ảnh phải có đuôi .jpg, .jpeg hoặc .png!");
                    }
                    if (file.ContentLength > 1000141)
                    {
                        ModelState.AddModelError("", "Hình ảnh phải có size < 1MB. Vui lòng cắt bớt hình hoặc chọn hình khác!");
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
                    if (post.Show)
                    {
                        HttpResponse.RemoveOutputCacheItem(Url.Action("Detail", "Post", new { alias = post.Alias, area = "" }));
                    }
                    if(ShowOld != true && post.Show)
                    {
                        HttpResponse.RemoveOutputCacheItem(Url.Action("PostByCategory", "Post", new { alias = post.Category.Alias, area = "" }));
                        HttpResponse.RemoveOutputCacheItem(Url.Action("PostBySubCategory", "Post", new { alias = post.SubCategory.Alias, area = "" }));
                    }
                    return RedirectToAction("Edit", new { id = post.PostID, page = page });
                    
                }
                ViewBag.UserID = new SelectList(db.AspNetUsers.Where(m => m.AspNetRoles.Count > 0), "Id", "Email", post.UserID);
                ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name", post.CategoryID);
                ViewBag.SubCategoryID = new SelectList(db.SubCategories.Where(m=>m.CategoryID == post.CategoryID), "ID", "Name", post.SubCategoryID);

                return View(post);
            }
            catch
            {
                ModelState.AddModelError("", "Xảy ra lỗi khi xử lý");
                ViewBag.UserID = new SelectList(db.AspNetUsers.Where(m => m.AspNetRoles.Count > 0), "Id", "Email", post.UserID);
                ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name", post.CategoryID);
                ViewBag.SubCategoryID = new SelectList(db.SubCategories.Where(m => m.CategoryID == post.CategoryID), "ID", "Name", post.SubCategoryID);
                return View(post);
            }
        }

        // GET: Admin/Post/Delete/5
        public ActionResult Delete(int? id)
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
            if (Session[User.Identity.GetUserId()] == null)
            {
                return RedirectToAction("ConfirmationContinue", "Account", new { area = "", Url = Request.Url, UrlReferrer = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : Url.Action("Index", "Home") });
            }
           
            return View(post);
        }

        // POST: Admin/Post/Delete/5
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
            var tags = post.Tags.ToList();
            ViewBag.Page = page ?? 1;
            return View(tags);
        }
        public ActionResult AddTags(int? id, int? page)
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
                var tagIDs = post.Tags.Select(m => m.TagID);
                ViewBag.TagID = new SelectList(db.Tags.Where(m => tagIDs.Contains(m.TagID) == false), "TagID", "Name");
                ViewBag.PostID = post.PostID;
                ViewBag.TitleArticle = post.Title;
                ViewBag.Page = page ?? 1;
                return View();

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddTags(List<int> TagID, int postID, int page)
        {
            try
            {
                Post post = db.Posts.Find(postID);
                foreach (int tagID in TagID)
                {
                    Tag tag = db.Tags.Find(tagID);
                    post.Tags.Add(tag);
                }
                db.SaveChanges();
                return RedirectToAction("TagsOfPost", new { id = postID, page = page });
            }
            catch
            {
                ModelState.AddModelError("", "Xảy ra lỗi khi xử lý");
                Post post = db.Posts.Find(postID);
                var tagIDs = post.Tags.Select(m => m.TagID);
                ViewBag.TagID = new SelectList(db.Tags.Where(m => tagIDs.Contains(m.TagID) == false), "TagID", "Name");
                ViewBag.PostID = postID;
                ViewBag.TitleArticle = post.Title;
                ViewBag.Page = page;
                return View();
            }
        }
        public ActionResult DeleteTag(int tagID, int postID, int? page)
        {
            Post post = db.Posts.Include(m => m.Tags).Single(m => m.PostID == postID);
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
