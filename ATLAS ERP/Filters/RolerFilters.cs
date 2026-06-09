using System.Linq;
using System.Web;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Infrastructure;

namespace ATLAS_ERP.Filters
{
    public class RoleFilter : ActionFilterAttribute
    {
        private readonly string[] roles;

        public RoleFilter(params string[] roles)
        {
            this.roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = HttpContext.Current.Session;

            if (session["UsuarioLogado"] == null)
            {
                filterContext.Result = new RedirectResult("/Auth/Login");
                return;
            }

            if (roles == null || roles.Length == 0)
                return;

            var cargoNome = session["CargoNome"]?.ToString();
            var usuarioId = (int?)session["UsuarioId"];

            if (string.IsNullOrEmpty(cargoNome) || !usuarioId.HasValue)
            {
                filterContext.Result = new RedirectResult("/Auth/Login");
                return;
            }

            if (!roles.Contains(cargoNome))
            {
                filterContext.Result = new RedirectResult("/Admin/Dashboard");
                return;
            }
        }
    }

    public class PermissaoFilter : ActionFilterAttribute
    {
        private readonly string[] permissoes;

        public PermissaoFilter(params string[] permissoes)
        {
            this.permissoes = permissoes;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = HttpContext.Current.Session;

            if (session["UsuarioLogado"] == null)
            {
                filterContext.Result = new RedirectResult("/Auth/Login");
                return;
            }

            if (permissoes == null || permissoes.Length == 0)
                return;

            var cargoId = (int?)session["CargoId"];
            if (!cargoId.HasValue)
            {
                filterContext.Result = new RedirectResult("/Auth/Login");
                return;
            }

            if (!PermissaoCache.TemAlguma(cargoId.Value, permissoes))
            {
                filterContext.Result = new RedirectResult("/Auth/Login");
                return;
            }
        }
    }
}