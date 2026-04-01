using System.Linq;
using System.Web;
using System.Web.Mvc;

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

            // 🔐 NÃO LOGADO → LOGIN
            if (session["UsuarioLogado"] == null)
            {
                filterContext.Result = new RedirectResult("/Auth/Login");
                return;
            }

            var userRole = session["Role"]?.ToString();

            // 🔥 SEM ROLE DEFINIDA → QUALQUER LOGADO ENTRA
            if (roles == null || roles.Length == 0)
                return;

            // 🔒 ROLE ERRADA → BLOQUEIA
            if (userRole == null || !roles.Contains(userRole))
            {
                filterContext.Result = new RedirectResult("/Admin/Dashboard");
            }
        }
    }
}