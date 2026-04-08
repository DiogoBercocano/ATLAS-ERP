using System.Diagnostics;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Filters;
using ATLAS_ERP.Models;
using ATLAS_ERP.Services;

namespace ATLAS_ERP.Controllers
{
    [RoleFilter("Admin")]
    public class UsuarioController : Controller
    {
        private readonly UsuarioService _service;
        private int EmpresaId => (int)Session[Infrastructure.SessionKeys.EmpresaId];

        public UsuarioController()
        {
            _service = new UsuarioService(new AtlasContext());
        }

        public ActionResult Index()
        {
            try
            {
                return View(_service.ListarPorEmpresa(EmpresaId));
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("UsuarioController.Index: {0}", ex);
                ViewBag.Erro = "Erro ao carregar funcionários.";
                return View(new System.Collections.Generic.List<Usuario>());
            }
        }

        public ActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Usuario user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    user.EmpresaId = EmpresaId;
                    _service.Criar(user);
                    return RedirectToAction("Index");
                }
                return View(user);
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("UsuarioController.Create: {0}", ex);
                ViewBag.Erro = "Erro ao cadastrar funcionário. Tente novamente.";
                return View(user);
            }
        }

        public ActionResult Edit() => RedirectToAction("Index");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int UsuarioId, string Name, string Email, string Role, string Ativo)
        {
            try
            {
                _service.Editar(UsuarioId, EmpresaId, Name, Email, Role, Ativo == "true");
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("UsuarioController.Edit: {0}", ex);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int usuarioId)
        {
            try
            {
                _service.Excluir(usuarioId, EmpresaId);
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("UsuarioController.Delete: {0}", ex);
                return RedirectToAction("Index");
            }
        }
    }
}
