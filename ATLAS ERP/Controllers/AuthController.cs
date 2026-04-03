using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Helpers;

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
            catch (Exception ex)
            {
                Trace.TraceError("[AuthController.Login GET] {0}", ex);
                return View();
            }
        }

        [HttpPost]
        public ActionResult Login(string email, string senha)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
                {
                    ViewBag.Erro = "Informe e-mail e senha.";
                    return View();
                }

                var user = db.Usuarios.FirstOrDefault(u =>
                    u.Email == email &&
                    u.Ativo == true
                );

                if (user != null && PasswordHelper.Verify(senha, user.SenhaHash))
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
            catch (Exception ex)
            {
                Trace.TraceError("[AuthController.Login POST] {0}", ex);
                ViewBag.Erro = "Erro ao realizar login. Tente novamente.";
                return View();
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
