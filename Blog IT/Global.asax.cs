using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Blog_IT.Models;
using System.Data.Entity;
using System.IO;
using System.Configuration;
using System.Web.Configuration;

namespace Blog_IT
{
    public class MvcApplication : System.Web.HttpApplication
    {
        static void EncryptConnectionString()
        {
            // Open the Web.config file.
            Configuration config = WebConfigurationManager.
                OpenWebConfiguration("~");

            // Get the connectionStrings section.
            ConnectionStringsSection section =
                config.GetSection("connectionStrings")
                as ConnectionStringsSection;

            // Toggle encryption.
            if (!section.SectionInformation.IsProtected)
            {
                section.SectionInformation.ProtectSection(
                  "DataProtectionConfigurationProvider");
            }

            // Save changes to the Web.config file.
            config.Save();
        }
        static void DecryptConnectionString()
        {
            // Open the Web.config file.
            Configuration config = WebConfigurationManager.
                OpenWebConfiguration("~");

            // Get the connectionStrings section.
            ConnectionStringsSection section =
                config.GetSection("connectionStrings")
                as ConnectionStringsSection;

            // Toggle encryption.
            if (section.SectionInformation.IsProtected)
            {
                section.SectionInformation.UnprotectSection();

            }

            // Save changes to the Web.config file.
            config.Save();
        }
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //EncryptConnectionString();
        }
        public override string GetVaryByCustomString(HttpContext context, string arg)
        {
            string cacheKey = string.Empty;

            string[] args = arg.Split(';');

            foreach (string item in args)
            {
                if (item == "alias" || item == "id" || item == "page")
                {
                    HttpContextBase currentContext = new HttpContextWrapper(HttpContext.Current);
                    RouteData routeData = RouteTable.Routes.GetRouteData(currentContext);

                    cacheKey += routeData.Values[item].ToString();
                }

                return cacheKey;
            }
            return base.GetVaryByCustomString(context, arg);
        }
        protected void Session_Start()
        {
            try
            {
                using (StreamReader r = new StreamReader(Server.MapPath("~/Content/files/luongtruycap.txt")))
                {
                    Application["luongtruycap"] = int.Parse(r.ReadToEnd());
                    Application["luongtruycap"] = (int)Application["luongtruycap"] + 1;
                }
                Application.Lock();
                using (StreamWriter w = new StreamWriter(Server.MapPath("~/Content/files/luongtruycap.txt")))
                {
                    w.Write(Application["luongtruycap"]);
                }
                Application.UnLock();
            }
            catch (Exception ex)
            {
                using (StreamWriter w = new StreamWriter(Server.MapPath("~/Content/files/errorlog.txt")))
                {
                    w.WriteLineAsync(ex.Message);
                }
            }

        }
 
    }
}
