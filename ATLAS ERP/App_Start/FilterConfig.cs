using System.Configuration;
using System.Web.Mvc;
using ATLAS_ERP.Filters;
using ATLAS_ERP.Infrastructure;

namespace ATLAS_ERP
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new GlobalExceptionFilter());

            // HTTPS enforcement controlado por appSetting Security:RequireHttps.
            // Em dev (Web.config) o valor é "false"; em Release (Web.Release.config) vira "true".
            // Mantém compatibilidade com IIS Express HTTP sem quebrar o fluxo.
            if (ShouldRequireHttps())
                filters.Add(new RequireHttpsRedirectAttribute());
        }

        private static bool ShouldRequireHttps()
        {
            var raw = ConfigurationManager.AppSettings["Security:RequireHttps"];
            bool flag;
            return bool.TryParse(raw, out flag) && flag;
        }
    }
}
