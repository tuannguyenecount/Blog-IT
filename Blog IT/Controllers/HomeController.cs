using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Blog_IT.Models;
using PagedList;
using PagedList.Mvc;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Globalization;
using System.Text;

namespace Blog_IT.Controllers
{
    public class HomeController : Controller
    {
        BlogITEntities db = new BlogITEntities();
        public class SitemapNode
        {
            public SitemapFrequency? Frequency { get; set; }
            public DateTime? LastModified { get; set; }
            public double? Priority { get; set; }
            public string Url { get; set; }
        }

        public enum SitemapFrequency
        {
            Never,
            Yearly,
            Monthly,
            Weekly,
            Daily,
            Hourly,
            Always
        }

        public IReadOnlyCollection<SitemapNode> GetSitemapNodes(UrlHelper urlHelper)
        {
            List<SitemapNode> nodes = new List<SitemapNode>();
            int countPage = (int)Math.Ceiling(db.Posts.Where(m => m.Show == true).Count() / 10.0);
            for(int page = 1; page <= countPage; page++)
            {
                nodes.Add(
                new SitemapNode()
                {
                    Url = Url.Action("Index", "Home", new { page = page }, this.Request.Url.Scheme),
                    Priority = 1,
                    Frequency = SitemapFrequency.Weekly
                });

            }

            nodes.Add(
               new SitemapNode()
               {
                   Url = Url.Action("About", "Home", null, this.Request.Url.Scheme),
                   Priority = 0.5
               });

            nodes.Add(
               new SitemapNode()
               {
                   Url = Url.Action("Contact", "Home", null, this.Request.Url.Scheme),
                   Priority = 0.5
               });

            foreach(string aliasPost in db.Posts.Select(m=>m.Alias))
            {
                nodes.Add( new SitemapNode()
                {
                 Url = Url.Action("Detail", "Post", new { alias = aliasPost }, this.Request.Url.Scheme),
                 Priority = 1
                });
            }
            foreach(Category category in db.Categories)
            {
                countPage = (int)Math.Ceiling(category.Posts.Where(m => m.Show == true).Count() / 10.0);
                for(int page = 1; page <= countPage;page++)
                {
                    nodes.Add(new SitemapNode()
                    {
                        Url = Url.Action("PostByCategory", "Post", new { alias = category.Alias, page = page }, this.Request.Url.Scheme),
                        Priority = 0.8
                    });
                }
            }
            foreach (SubCategory subcategory in db.SubCategories)
            {
                countPage = (int)Math.Ceiling(subcategory.Posts.Where(m => m.Show == true).Count() / 10.0);
                for (int page = 1; page <= countPage; page++)
                {
                    nodes.Add(new SitemapNode()
                    {
                        Url = Url.Action("PostBySubCategory", "Post", new { alias = subcategory.Alias, page = page }, this.Request.Url.Scheme),
                        Priority = 0.8
                    });
                }
            }

            foreach (Tag tag in db.Tags)
            {
                countPage = (int)Math.Ceiling(tag.Posts.Where(m => m.Show == true).Count() / 10.0);
                for (int page = 1; page <= countPage; page++)
                {
                    nodes.Add(new SitemapNode()
                    {
                        Url = Url.Action("Index", "Tag", new { id = tag.TagID, alias = tag.Alias, page = page }, this.Request.Url.Scheme),
                        Priority = 0.8
                    });
                }
            }
            return nodes;
        }
        
        public string GetSitemapDocument(IEnumerable<SitemapNode> sitemapNodes)
        {
            XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            XElement root = new XElement(xmlns + "urlset");

            foreach (SitemapNode sitemapNode in sitemapNodes)
            {
                XElement urlElement = new XElement(
                    xmlns + "url",
                    new XElement(xmlns + "loc", Uri.EscapeUriString(sitemapNode.Url)),
                    sitemapNode.LastModified == null ? null : new XElement(
                        xmlns + "lastmod",
                        sitemapNode.LastModified.Value.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:sszzz")),
                    sitemapNode.Frequency == null ? null : new XElement(
                        xmlns + "changefreq",
                        sitemapNode.Frequency.Value.ToString().ToLowerInvariant()),
                    sitemapNode.Priority == null ? null : new XElement(
                        xmlns + "priority",
                        sitemapNode.Priority.Value.ToString("F1", CultureInfo.InvariantCulture)));
                root.Add(urlElement);
            }

            XDocument document = new XDocument(root);
            return document.ToString();
        }

        [Route("sitemap.xml")]
        public ContentResult SitemapXML()
        {
            var sitemapNodes = GetSitemapNodes(this.Url);
            string xml = GetSitemapDocument(sitemapNodes);
            return Content(xml, "text/xml");
        }
        public ActionResult Index(int? page)
        {
            var newPosts = db.Posts.Where(m => m.Show == true).OrderByDescending(p => p.PostID);
            return View(newPosts.ToPagedList(page ?? 1, 10));
        }

        public ActionResult About()
        {
            return View();
        }

        public PartialViewResult LayNoiDungBangTin()
        {
            return PartialView(db.Popups.First());
        }

        [ChildActionOnly]
        public CaptchaImageResult ShowCaptchaImage()
        {
            return new CaptchaImageResult();
        }
        public ActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(Mailbox model)
        {
            model.SendDate = DateTime.Now;
            if(ModelState.IsValid)
            {
                db.Mailboxes.Add(model);
                db.SaveChanges();
                //ViewBag.MessageSuccess = "Gửi thành công.";
                return RedirectToAction("ContactSuccess");
            }
            return View(model);
        }

        [ChildActionOnly]
        [OutputCache(Duration = 86400)]
        public PartialViewResult _TopMenuPartial()
        {
            return PartialView("_TopMenuPartial", db.Categories.OrderBy(m=>m.STT).AsEnumerable());
        }
        
        public ViewResult ContactSuccess()
        {
            return View();
        }
        [ChildActionOnly]
        [OutputCache(Duration = 86400)]
        public PartialViewResult _HinhGanDayPartial()
        {
            return PartialView(db.Posts.Where(m=>m.Show == true).OrderByDescending(m => m.PostID).Take(9).Select(m=>m.Image));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DangKyNhanTin(MailSubscribe mailSubscribe)
        {
            try
            {
                if(mailSubscribe.Email.Trim() == string.Empty || mailSubscribe.FullName.Trim() == string.Empty)
                {
                    ViewBag.Result = 0;
                    ViewBag.ErrorMessage = "Vui lòng nhập đủ thông tin!";
                    ViewBag.Title = "Lỗi";
                    return View("SubscribeConfirmation");
                }
                else if(mailSubscribe.FullName.Trim().Length > 200)
                {
                    ViewBag.Result = 0;
                    ViewBag.ErrorMessage = "Họ tên quá dài!";
                    ViewBag.Title = "Lỗi";
                    return View("SubscribeConfirmation");
                }
                else if(App_Code.IsValidEmail(mailSubscribe.Email) == false)
                {
                    ViewBag.Result = 0;
                    ViewBag.ErrorMessage = "Địa chỉ email không hợp lệ!";
                    ViewBag.Title = "Lỗi";
                    return View("SubscribeConfirmation");
                }
                mailSubscribe.DateSubscribe = DateTime.Now;
                if(await db.MailSubscribes.FindAsync(mailSubscribe.Email) == null)
                { 
                    db.MailSubscribes.Add(mailSubscribe);
                    ViewBag.Result = 1;
                    ViewBag.Title = "Thành công";
                    await db.SaveChangesAsync();
                }
                else
                {
                    ViewBag.Result = 2;
                    ViewBag.Title = "Lỗi";
                }
                
                return View("SubscribeConfirmation");
            }
            catch
            {
                ViewBag.Result = 0;
                ViewBag.ErrorMessage = "Xảy ra lỗi khi xử lý";
                ViewBag.Title = "Lỗi";
                return View("SubscribeConfirmation");
            }
        }
        [ChildActionOnly]
        public ActionResult _BangTinPartial()
        {
            if(db.Popups.First().Show == true)
            return PartialView(db.Popups.First());
            else
            {
                return Content("");
            }
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