using System.Diagnostics;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Filters;
using ATLAS_ERP.Models;
using ATLAS_ERP.Services;

namespace ATLAS_ERP.Controllers
{
    public class ProdutoController : Controller
    {
        private readonly ProdutoService _service;
        private int EmpresaId => (int)Session[Infrastructure.SessionKeys.EmpresaId];

        public ProdutoController()
        {
            _service = new ProdutoService(new AtlasContext());
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
                Trace.TraceError("ProdutoController.Index: {0}", ex);
                ViewBag.Erro = "Erro ao carregar produtos.";
                return View(new System.Collections.Generic.List<Produto>());
            }
        }

        [RoleFilter("Admin", "Gerente")]
        public ActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleFilter("Admin", "Gerente")]
        public ActionResult Create(Produto produto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    produto.EmpresaId = EmpresaId;
                    _service.Criar(produto);
                    return RedirectToAction("Index");
                }
                return View(produto);
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("ProdutoController.Create: {0}", ex);
                ViewBag.Erro = "Erro ao cadastrar produto. Tente novamente.";
                return View(produto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleFilter("Admin", "Gerente")]
        public ActionResult Edit(int ProdutoId, string Nome, string Categoria,
                                 decimal PrecoVenda, int EstoqueMinimo, bool Ativo)
        {
            try
            {
                _service.Editar(ProdutoId, EmpresaId, Nome, Categoria, PrecoVenda, EstoqueMinimo, Ativo);
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("ProdutoController.Edit: {0}", ex);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleFilter("Admin")]
        public ActionResult Delete(int produtoId)
        {
            try
            {
                _service.Excluir(produtoId, EmpresaId);
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("ProdutoController.Delete: {0}", ex);
                return RedirectToAction("Index");
            }
        }
    }
}
