using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Helpers;
using ATLAS_ERP.Models;
using ATLAS_ERP.Filters;

namespace ATLAS_ERP.Controllers
{
    [RoleFilter("Admin")]
    public class UsuarioController : Controller
    {
        private readonly AtlasContext db = new AtlasContext();

        private int? EmpresaIdOrNull => Session["EmpresaId"] as int?;

        private static readonly string[] RolesPermitidas = { "Admin", "Gerente", "Funcionario" };

        public ActionResult Index()
        {
            try
            {
                var empresaId = EmpresaIdOrNull;
                if (empresaId == null)
                    return RedirectToAction("Login", "Auth");

                var usuarios = db.Usuarios.Where(u => u.EmpresaId == empresaId.Value).ToList();
                return View(usuarios);
            }
            catch (Exception ex)
            {
                Trace.TraceError("[UsuarioController.Index] {0}", ex);
                ViewBag.Erro = "Erro ao carregar funcionários.";
                return View(new System.Collections.Generic.List<Usuario>());
            }
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Usuario user)
        {
            try
            {
                var empresaId = EmpresaIdOrNull;
                if (empresaId == null)
                    return RedirectToAction("Login", "Auth");

                if (ModelState.IsValid)
                {
                    user.Ativo = true;
                    user.EmpresaId = empresaId.Value;
                    user.SenhaHash = PasswordHelper.Hash(user.SenhaHash);
                    db.Usuarios.Add(user);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(user);
            }
            catch (Exception ex)
            {
                Trace.TraceError("[UsuarioController.Create POST] {0}", ex);
                ViewBag.Erro = "Erro ao cadastrar funcionário. Tente novamente.";
                return View(user);
            }
        }

        [HttpPost]
        public ActionResult Edit(int usuarioId, string Name, string Email, string Role, bool Ativo)
        {
            try
            {
                var empresaId = EmpresaIdOrNull;
                if (empresaId == null)
                    return RedirectToAction("Login", "Auth");

                if (!Array.Exists(RolesPermitidas, r => r == Role))
                {
                    TempData["Erro"] = "Perfil de acesso inválido.";
                    return RedirectToAction("Index");
                }

                var u = db.Usuarios.FirstOrDefault(x => x.UsuarioId == usuarioId && x.EmpresaId == empresaId.Value);
                if (u != null)
                {
                    u.Name = Name;
                    u.Email = Email;
                    u.Role = Role;
                    u.Ativo = Ativo;
                    db.Entry(u).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Trace.TraceError("[UsuarioController.Edit POST] {0}", ex);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Delete(int usuarioId)
        {
            try
            {
                var empresaId = EmpresaIdOrNull;
                if (empresaId == null)
                    return RedirectToAction("Login", "Auth");

                var u = db.Usuarios.FirstOrDefault(x => x.UsuarioId == usuarioId && x.EmpresaId == empresaId.Value);
                if (u != null)
                {
                    db.Usuarios.Remove(u);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Trace.TraceError("[UsuarioController.Delete POST] {0}", ex);
                return RedirectToAction("Index");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
