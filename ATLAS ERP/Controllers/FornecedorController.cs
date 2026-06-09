using System.Collections.Generic;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Filters;
using ATLAS_ERP.Infrastructure;
using ATLAS_ERP.Models;
using ATLAS_ERP.Services;

namespace ATLAS_ERP.Controllers
{
    public class FornecedorController : Controller
    {
        private readonly AtlasContext      _db;
        private readonly FornecedorService _service;
        private int EmpresaId => (int)Session[Infrastructure.SessionKeys.EmpresaId];

        public FornecedorController()
        {
            _db      = new AtlasContext();
            _service = new FornecedorService(_db);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db?.Dispose();
            base.Dispose(disposing);
        }

        [PermissaoFilter("fornecedores_view")]
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
                AppLogger.Error(ex, "FornecedorController.Index");
                ViewBag.Erro = "Erro ao carregar fornecedores.";
                return View(PagedResult<Fornecedor>.Empty(pageSize));
            }
        }

        [PermissaoFilter("fornecedores_create")]
        public ActionResult Create() => View();

        [PermissaoFilter("fornecedores_edit")]
        public ActionResult Edit(int id)
        {
            var fornecedor = _service.BuscarPorId(id, EmpresaId);
            if (fornecedor == null)
            {
                TempData["Erro"] = "Fornecedor não encontrado.";
                return RedirectToAction("Index");
            }
            return View(fornecedor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissaoFilter("fornecedores_create")]
        public ActionResult Create(Fornecedor fornecedor)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    fornecedor.EmpresaId = EmpresaId;
                    _service.Criar(fornecedor);
                    return RedirectToAction("Index");
                }
                return View(fornecedor);
            }
            catch (DomainException dex)
            {
                ViewBag.Erro = dex.Message;
                return View(fornecedor);
            }
            catch (System.Exception ex)
            {
                AppLogger.Error(ex, "FornecedorController.Create");
                ViewBag.Erro = "Erro ao cadastrar fornecedor. Tente novamente.";
                return View(fornecedor);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissaoFilter("fornecedores_edit")]
        public ActionResult Edit(int FornecedorId, string Nome, string CNPJ, string Telefone, string Email)
        {
            try
            {
                _service.Editar(FornecedorId, EmpresaId, Nome, CNPJ, Telefone, Email);
                return RedirectToAction("Index");
            }
            catch (DomainException dex)
            {
                TempData["Erro"] = dex.Message;
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                AppLogger.Error(ex, "FornecedorController.Edit");
                TempData["Erro"] = "Erro ao editar fornecedor.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissaoFilter("fornecedores_delete")]
        public ActionResult Delete(int fornecedorId)
        {
            try
            {
                _service.Excluir(fornecedorId, EmpresaId);
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                AppLogger.Error(ex, "FornecedorController.Delete");
                return RedirectToAction("Index");
            }
        }
    }
}
