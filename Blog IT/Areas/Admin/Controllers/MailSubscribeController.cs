using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Blog_IT.Models;
using System.IO;
using System.Net;
namespace Blog_IT.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MailSubscribeController : Controller
    {
        BlogITEntities db = new BlogITEntities();
        // GET: Admin/MailSubscribe
        public ActionResult Index()
        {
            return View(db.MailSubscribes.AsEnumerable());
        }
        public FileResult Download()
        {
            string[] dsMail = db.MailSubscribes.Select(m => m.Email).ToArray();
            using (StreamWriter w = new StreamWriter(Server.MapPath("~/Content/files/dsmail.txt")))
            {
                w.Write(string.Join(" ", dsMail));
            }
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Content/files/dsmail.txt"));
            string fileName = "dsmail.txt";
            System.IO.File.Delete(Server.MapPath("~/Content/files/dsmail.txt"));
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
        [HttpPost]
        [ValidateInput(false)]
        public int Delete(string Email)
        {
            try
            {
                Email = HttpUtility.HtmlDecode(Email);

                MailSubscribe mail = db.MailSubscribes.Find(Email);
                db.MailSubscribes.Remove(mail);
                db.SaveChanges();
                return 1;
            }
            catch
            {
                return 0;
            }
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