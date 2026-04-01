using System.Linq;
using System.Web.Mvc;
using ATLAS_ERP.Data;

namespace ATLAS_ERP.Controllers
{
    public class AuthController : Controller
    {
        private readonly AtlasContext db = new AtlasContext();

        // GET
        public ActionResult Login()
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

        // POST
        [HttpPost]
        public ActionResult Login(string email, string senha)
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

        // LOGOUT
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}