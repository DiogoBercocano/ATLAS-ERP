using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Filters;
using ATLAS_ERP.Infrastructure;
using ATLAS_ERP.Models;
using ATLAS_ERP.Services;

namespace ATLAS_ERP.Controllers
{
    public class ClienteController : Controller
    {
        private readonly AtlasContext   _db;
        private readonly ClienteService _service;
        private int EmpresaId => (int)Session[Infrastructure.SessionKeys.EmpresaId];

        public ClienteController()
        {
            _db      = new AtlasContext();
            _service = new ClienteService(_db);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db?.Dispose();
            base.Dispose(disposing);
        }

        [PermissaoFilter("clientes_view")]
        public ActionResult Index(int page = 1, int pageSize = PagingDefaults.PageSize, string search = null)
        {
            if (Session[Infrastructure.SessionKeys.UsuarioLogado] == null)
                return RedirectToAction("Login", "Auth");
            try
            {
                return View(_service.ListarPaginado(EmpresaId, page, pageSize, search));
            }
            catch (System.Exception ex)
            {
                AppLogger.Error(ex, "ClienteController.Index");
                ViewBag.Erro = "Erro ao carregar clientes.";
                return View(PagedResult<Cliente>.Empty(pageSize));
            }
        }

        [PermissaoFilter("clientes_create")]
        public ActionResult Create() => View();

        [PermissaoFilter("clientes_edit")]
        public ActionResult Edit(int id)
        {
            var cliente = _service.BuscarPorId(id, EmpresaId);
            if (cliente == null)
            {
                TempData["Erro"] = "Cliente não encontrado.";
                return RedirectToAction("Index");
            }
            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissaoFilter("clientes_create")]
        public ActionResult Create(Cliente cliente)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    cliente.EmpresaId = EmpresaId;
                    _service.Criar(cliente);
                    return RedirectToAction("Index");
                }
                return View(cliente);
            }
            catch (DomainException dex)
            {
                ViewBag.Erro = dex.Message;
                return View(cliente);
            }
            catch (System.Exception ex)
            {
                AppLogger.Error(ex, "ClienteController.Create");
                ViewBag.Erro = "Erro ao cadastrar cliente. Tente novamente.";
                return View(cliente);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissaoFilter("clientes_edit")]
        public ActionResult Edit(int ClienteId, string Nome, string Documento,
                                 string Email, string Telefone, string Endereco,
                                 decimal? LimiteCredito, string Ativo)
        {
            try
            {
                _service.Editar(ClienteId, EmpresaId, Nome, Documento, Email,
                                Telefone, Endereco, LimiteCredito ?? 0, Ativo == "true");
                return RedirectToAction("Index");
            }
            catch (DomainException dex)
            {
                TempData["Erro"] = dex.Message;
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                AppLogger.Error(ex, "ClienteController.Edit");
                TempData["Erro"] = "Erro ao editar cliente.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissaoFilter("clientes_delete")]
        public ActionResult Delete(int clienteId)
        {
            try
            {
                _service.Excluir(clienteId, EmpresaId);
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                AppLogger.Error(ex, "ClienteController.Delete");
                return RedirectToAction("Index");
            }
        }
    }
}
