using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Blog_IT.Controllers
{
    public class StaticContentController : Controller
    {
        // GET: StaticContent
        
        public ViewResult PageNotFound()
        {
            return View();
        }
        public ViewResult Error()
        {
            return View();
        }

    }
}