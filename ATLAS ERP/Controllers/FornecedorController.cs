using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Filters;
using ATLAS_ERP.Models;
using ATLAS_ERP.Services;

namespace ATLAS_ERP.Controllers
{
    public class FornecedorController : Controller
    {
        private readonly FornecedorService _service;
        private int EmpresaId => (int)Session[Infrastructure.SessionKeys.EmpresaId];

        public FornecedorController()
        {
            _service = new FornecedorService(new AtlasContext());
        }

        public ActionResult Index()
        {
            if (Session[Infrastructure.SessionKeys.UsuarioLogado] == null)
                return RedirectToAction("Login", "Auth");
            try
            {
                return View(_service.ListarPorEmpresa(EmpresaId));
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("FornecedorController.Index: {0}", ex);
                ViewBag.Erro = "Erro ao carregar fornecedores.";
                return View(new List<Fornecedor>());
            }
        }

        [RoleFilter("Admin", "Gerente")]
        public ActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleFilter("Admin", "Gerente")]
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
            catch (System.Exception ex)
            {
                Trace.TraceError("FornecedorController.Create: {0}", ex);
                ViewBag.Erro = "Erro ao cadastrar fornecedor. Tente novamente.";
                return View(fornecedor);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleFilter("Admin", "Gerente")]
        public ActionResult Edit(int FornecedorId, string Nome, string CNPJ, string Telefone, string Email)
        {
            try
            {
                _service.Editar(FornecedorId, EmpresaId, Nome, CNPJ, Telefone, Email);
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("FornecedorController.Edit: {0}", ex);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleFilter("Admin")]
        public ActionResult Delete(int fornecedorId)
        {
            try
            {
                _service.Excluir(fornecedorId, EmpresaId);
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("FornecedorController.Delete: {0}", ex);
                return RedirectToAction("Index");
            }
        }
    }
}
