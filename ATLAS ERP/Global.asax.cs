using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ATLAS_ERP.Data;
using ATLAS_ERP.Infrastructure;
using ATLAS_ERP.Migrations;

namespace ATLAS_ERP
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Garante TLS 1.2 para chamadas HttpClient a APIs externas (ViaCEP, BrasilAPI, ReceitaWS).
            // Deve ser definido ANTES do primeiro uso de HttpClient para ter efeito.
            System.Net.ServicePointManager.SecurityProtocol =
                System.Net.SecurityProtocolType.Tls12 |
                System.Net.SecurityProtocolType.Tls11;

            // Aplica automaticamente migrations pendentes ao iniciar o app.
            // Substitui o inicializador padrão (CreateDatabaseIfNotExists) que criava tabelas
            // sem registrar no __MigrationHistory, causando conflito com o PM Console.
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<AtlasContext, Configuration>());

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
            // Força InvariantCulture em todos os requests para que o model binder
            // aceite ponto como separador decimal (formato enviado pelo JavaScript).
            System.Threading.Thread.CurrentThread.CurrentCulture   = System.Globalization.CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

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
