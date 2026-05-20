using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ATLAS_ERP.Infrastructure;

namespace ATLAS_ERP
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Remove "X-AspNetMvc-Version" do response (fingerprint da stack).
            // "X-AspNet-Version" já é controlado por <httpRuntime enableVersionHeader="false">.
            MvcHandler.DisableMvcResponseHeader = true;

            // Antiforgery token endurecido: cookie com nome custom para não vazar stack.
            // RequireSsl segue o flag de HTTPS enforcement (false em dev, true em Release).
            // SuppressXFrameOptionsHeader = true → SecurityHeadersModule já emite DENY.
            AntiForgeryConfig.CookieName = "ATLAS.AFT";
            AntiForgeryConfig.RequireSsl = RequireSslFromConfig();
            AntiForgeryConfig.SuppressXFrameOptionsHeader = true;

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AppLogger.Info("application_start version={0}", typeof(MvcApplication).Assembly.GetName().Version);
        }

        protected void Application_BeginRequest()
        {
            if (HttpContext.Current != null && HttpContext.Current.Items["RequestId"] == null)
                HttpContext.Current.Items["RequestId"] = Guid.NewGuid().ToString("N").Substring(0, 12);
        }

        protected void Application_Error()
        {
            var ex = Server.GetLastError();
            if (ex != null)
                AppLogger.Error(ex, "unhandled_exception");
        }

        private static bool RequireSslFromConfig()
        {
            var raw = System.Configuration.ConfigurationManager.AppSettings["Security:RequireHttps"];
            bool flag;
            return bool.TryParse(raw, out flag) && flag;
        }
    }
}
