using System.Diagnostics;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Filters;
using ATLAS_ERP.Infrastructure;
using ATLAS_ERP.Models;
using ATLAS_ERP.Services;

namespace ATLAS_ERP.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly AtlasContext   _db;
        private readonly UsuarioService _service;
        private readonly CargoService   _cargoService;
        private int EmpresaId => (int)Session[Infrastructure.SessionKeys.EmpresaId];

        public UsuarioController()
        {
            _db           = new AtlasContext();
            _service      = new UsuarioService(_db);
            _cargoService = new CargoService(_db);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db?.Dispose();
            base.Dispose(disposing);
        }

        [PermissaoFilter("usuarios_view")]
        public ActionResult Index(int page = 1, int pageSize = PagingDefaults.PageSize, string search = null)
        {
            try
            {
                return View(_service.ListarPaginado(EmpresaId, page, pageSize, search));
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("UsuarioController.Index: {0}", ex);
                ViewBag.Erro = "Erro ao carregar funcionários.";
                return View(PagedResult<Usuario>.Empty(pageSize));
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
                {
                    var usuarioLogadoId = Session[Infrastructure.SessionKeys.UsuarioId] as int?;
                    if (usuarioLogadoId.HasValue && usuarioLogadoId.Value == UsuarioId)
                    {
                        Session[Infrastructure.SessionKeys.CargoId] = CargoId;
                        PermissaoCache.InvalidarSessao(Session);
                    }
                    return RedirectToAction("Index");
                }
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
