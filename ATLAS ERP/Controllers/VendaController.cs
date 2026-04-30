using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Filters;
using ATLAS_ERP.Services;

namespace ATLAS_ERP.Controllers
{
    public class VendaController : Controller
    {
        private readonly VendaService    _vendaService;
        private readonly ClienteService  _clienteService;
        private readonly ProdutoService  _produtoService;
        private readonly RelatorioService _relatorioService;

        private int EmpresaId  => (int)Session[Infrastructure.SessionKeys.EmpresaId];
        private int UsuarioId  => (int)Session[Infrastructure.SessionKeys.UsuarioId];

        public VendaController()
        {
            var db = new AtlasContext();
            _vendaService     = new VendaService(db);
            _clienteService   = new ClienteService(db);
            _produtoService   = new ProdutoService(db);
            _relatorioService = new RelatorioService(db);
        }

        [PermissaoFilter("vendas_view")]
        public ActionResult Index()
        {
            try
            {
                return View(_vendaService.ListarPorEmpresa(EmpresaId));
            }
            catch (Exception ex)
            {
                Trace.TraceError("VendaController.Index: {0}", ex);
                ViewBag.Erro = "Erro ao carregar vendas.";
                return View(new List<Models.Venda>());
            }
        }

        [PermissaoFilter("vendas_create")]
        public ActionResult Create()
        {
            ViewBag.Clientes = _clienteService.ListarPorEmpresa(EmpresaId);
            ViewBag.Produtos = _produtoService.ListarAtivos(EmpresaId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissaoFilter("vendas_create")]
        public ActionResult Create(int clienteId, DateTime vencimento,
                                   int[] produtoIds, int[] quantidades, decimal[] precos)
        {
            try
            {
                if (produtoIds == null || produtoIds.Length == 0)
                {
                    ViewBag.Erro     = "Adicione ao menos um produto à venda.";
                    ViewBag.Clientes = _clienteService.ListarPorEmpresa(EmpresaId);
                    ViewBag.Produtos = _produtoService.ListarAtivos(EmpresaId);
                    return View();
                }

                var itens = new List<Services.VendaItemDto>();
                for (int i = 0; i < produtoIds.Length; i++)
                    itens.Add(new Services.VendaItemDto { ProdutoId = produtoIds[i], Quantidade = quantidades[i], Preco = precos[i] });

                var venda = _vendaService.Criar(EmpresaId, clienteId, UsuarioId, vencimento, itens);
                return RedirectToAction("Nota", new { id = venda.VendaId });
            }
            catch (Exception ex)
            {
                Trace.TraceError("VendaController.Create: {0}", ex);
                ViewBag.Erro     = "Erro ao registrar venda. Tente novamente.";
                ViewBag.Clientes = _clienteService.ListarPorEmpresa(EmpresaId);
                ViewBag.Produtos = _produtoService.ListarAtivos(EmpresaId);
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissaoFilter("vendas_delete")]
        public ActionResult Cancelar(int vendaId)
        {
            try
            {
                _vendaService.Cancelar(vendaId, EmpresaId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Trace.TraceError("VendaController.Cancelar: {0}", ex);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissaoFilter("vendas_edit")]
        public ActionResult RegistrarPagamento(int contaId)
        {
            try
            {
                _vendaService.RegistrarPagamento(contaId, EmpresaId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Trace.TraceError("VendaController.RegistrarPagamento: {0}", ex);
                return RedirectToAction("Index");
            }
        }

        [PermissaoFilter("vendas_view")]
        public ActionResult Nota(int id)
        {
            try
            {
                var venda = _vendaService.BuscarComItens(id, EmpresaId);
                if (venda == null) return HttpNotFound();
                return View(venda);
            }
            catch (Exception ex)
            {
                Trace.TraceError("VendaController.Nota: {0}", ex);
                return RedirectToAction("Index");
            }
        }

        [PermissaoFilter("vendas_view")]
        public ActionResult Financeiro()
        {
            try
            {
                var resumo = _relatorioService.GerarResumo(EmpresaId);
                return View(resumo);
            }
            catch (Exception ex)
            {
                Trace.TraceError("VendaController.Financeiro: {0}", ex);
                return RedirectToAction("Index");
            }
        }
    }
}
