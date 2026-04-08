using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Helpers;
using ATLAS_ERP.Infrastructure;

namespace ATLAS_ERP.Controllers
{
    public class AuthController : Controller
    {
        private readonly AtlasContext db = new AtlasContext();

        public ActionResult Login()
        {
            try
            {
                if (Session[SessionKeys.UsuarioLogado] != null)
                {
                    var role = Session[SessionKeys.Role]?.ToString();
                    if (role == Roles.SuperAdmin)
                        return RedirectToAction("Index", "SuperAdmin");
                    if (role == Roles.Admin || role == Roles.Gerente)
                        return RedirectToAction("Dashboard", "Admin");
                    return RedirectToAction("Index", "Produto");
                }
                return View();
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("AuthController.Login GET: {0}", ex);
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string senha)
        {
            try
            {
                var senhaHash = PasswordHelper.HashSenha(senha);
                var user = db.Usuarios.FirstOrDefault(u =>
                    u.Email == email &&
                    u.SenhaHash == senhaHash &&
                    u.Ativo == true
                );

                if (user != null)
                {
                    Session[SessionKeys.UsuarioLogado] = user.Name;
                    Session[SessionKeys.UsuarioId]     = user.UsuarioId;
                    Session[SessionKeys.Role]          = user.Role;
                    Session[SessionKeys.EmpresaId]     = user.EmpresaId.HasValue ? (object)user.EmpresaId.Value : null;

                    if (user.Role == Roles.SuperAdmin)
                        return RedirectToAction("Index", "SuperAdmin");
                    if (user.Role == Roles.Admin || user.Role == Roles.Gerente)
                        return RedirectToAction("Dashboard", "Admin");

                    return RedirectToAction("Index", "Produto");
                }

                ViewBag.Erro = "E-mail ou senha inválidos.";
                return View();
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("AuthController.Login POST: {0}", ex);
                ViewBag.Erro = "Erro ao realizar login. Tente novamente.";
                return View();
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
