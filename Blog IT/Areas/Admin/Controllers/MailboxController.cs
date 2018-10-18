using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Blog_IT.Models;
using System.Threading.Tasks;

namespace Blog_IT.Areas.Admin.Controllers
{
    [Authorize(Roles="Admin")]
    public class MailboxController : Controller
    {
        private BlogITEntities db = new BlogITEntities();

        // GET: Admin/Mailbox
        public ActionResult Index()
        {
            return View(db.Mailboxes.OrderBy(m=>m.Confirmed).ToList());
        }

        // GET: Admin/Mailbox/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Mailbox mailbox = db.Mailboxes.Find(id);
            if (mailbox == null)
            {
                return RedirectToAction("PageNotFound", "StaticContent", new { area = "" });
            }
            return View(mailbox);
        }

        // POST: Admin/Mailbox/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Mailbox mailbox = db.Mailboxes.Find(id);
            db.Mailboxes.Remove(mailbox);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> ChangeConfirm(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Mailbox mailbox = db.Mailboxes.Find(id);
            if (mailbox == null)
            {
                return RedirectToAction("PageNotFound", "StaticContent", new { area = "" });
            }
            mailbox.Confirmed = !mailbox.Confirmed;
            db.Entry(mailbox).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
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
