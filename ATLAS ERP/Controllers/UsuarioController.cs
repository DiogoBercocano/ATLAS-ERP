using System.Diagnostics;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Filters;
using ATLAS_ERP.Models;
using ATLAS_ERP.Services;

namespace ATLAS_ERP.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly UsuarioService _service;
        private readonly CargoService _cargoService;
        private int EmpresaId => (int)Session[Infrastructure.SessionKeys.EmpresaId];

        public UsuarioController()
        {
            var context = new AtlasContext();
            _service = new UsuarioService(context);
            _cargoService = new CargoService(context);
        }

        [PermissaoFilter("usuarios_view")]
        public ActionResult Index()
        {
            try
            {
                var usuarios = _service.ListarPorEmpresa(EmpresaId);
                return View(usuarios);
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("UsuarioController.Index: {0}", ex);
                ViewBag.Erro = $"Erro ao carregar funcionários: {ex.Message}";
                return View(new System.Collections.Generic.List<Usuario>());
            }
        }

        [PermissaoFilter("usuarios_create")]
        public ActionResult Create()
        {
            try
            {
                var cargos = _cargoService.ListarPorEmpresa(EmpresaId);
                ViewBag.Cargos = cargos ?? new System.Collections.Generic.List<Models.Cargo>();
                return View();
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("UsuarioController.Create GET: {0}", ex);
                ViewBag.Erro = $"Erro ao carregar cargos: {ex.Message}";
                ViewBag.Cargos = new System.Collections.Generic.List<Models.Cargo>();
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissaoFilter("usuarios_create")]
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
                ViewBag.Cargos = _cargoService.ListarPorEmpresa(EmpresaId);
                return View(user);
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("UsuarioController.Create POST: {0}", ex);
                ViewBag.Erro = $"Erro ao cadastrar funcionário: {ex.Message}";
                ViewBag.Cargos = _cargoService.ListarPorEmpresa(EmpresaId);
                return View(user);
            }
        }

        public ActionResult Edit() => RedirectToAction("Index");

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissaoFilter("usuarios_edit")]
        public ActionResult Edit(int UsuarioId, string Name, string Email, int? CargoId, string Ativo)
        {
            try
            {
                var resultado = _service.Editar(UsuarioId, EmpresaId, Name, Email, CargoId, Ativo == "true");
                if (resultado)
                    return RedirectToAction("Index");
                else
                {
                    ViewBag.Erro = "Funcionário não encontrado.";
                    return RedirectToAction("Index");
                }
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("UsuarioController.Edit: {0}", ex);
                ViewBag.Erro = $"Erro ao editar: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissaoFilter("usuarios_delete")]
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
