using System.Linq;
using System.Web.Mvc;
using ATLAS_ERP.Data;

namespace ATLAS_ERP.Controllers
{
    public class AuthController : Controller
    {
        private readonly AtlasContext db = new AtlasContext();

        public ActionResult Login()
        {
            try
            {
                if (Session["UsuarioLogado"] != null)
                {
                    var role = Session["Role"]?.ToString();
                    if (role == "SuperAdmin")
                        return RedirectToAction("Index", "SuperAdmin");
                    if (role == "Admin" || role == "Gerente")
                        return RedirectToAction("Dashboard", "Admin");
                    return RedirectToAction("Index", "Produto");
                }
                return View();
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult Login(string email, string senha)
        {
            try
            {
                var user = db.Usuarios.FirstOrDefault(u =>
                    u.Email == email &&
                    u.SenhaHash == senha &&
                    u.Ativo == true
                );

                if (user != null)
                {
                    Session["UsuarioLogado"] = user.Name;
                    Session["UsuarioId"] = user.UsuarioId;
                    Session["Role"] = user.Role;

                    if (user.EmpresaId.HasValue)
                        Session["EmpresaId"] = user.EmpresaId.Value;
                    else
                        Session["EmpresaId"] = null;

                    if (user.Role == "SuperAdmin")
                        return RedirectToAction("Index", "SuperAdmin");
                    if (user.Role == "Admin" || user.Role == "Gerente")
                        return RedirectToAction("Dashboard", "Admin");

                    return RedirectToAction("Index", "Produto");
                }

                ViewBag.Erro = "E-mail ou senha inválidos.";
                return View();
            }
            catch
            {
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