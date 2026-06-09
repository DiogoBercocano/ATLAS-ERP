using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Filters;
using ATLAS_ERP.Infrastructure;
using ATLAS_ERP.Services;

namespace ATLAS_ERP.Controllers
{
    public class VendaController : Controller
    {
        private readonly AtlasContext     _db;
        private readonly VendaService     _vendaService;
        private readonly ClienteService   _clienteService;
        private readonly ProdutoService   _produtoService;
        private readonly RelatorioService _relatorioService;

        private int EmpresaId  => (int)Session[Infrastructure.SessionKeys.EmpresaId];
        private int UsuarioId  => (int)Session[Infrastructure.SessionKeys.UsuarioId];

        public VendaController()
        {
            _db               = new AtlasContext();
            _vendaService     = new VendaService(_db);
            _clienteService   = new ClienteService(_db);
            _produtoService   = new ProdutoService(_db);
            _relatorioService = new RelatorioService(_db);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db?.Dispose();
            base.Dispose(disposing);
        }

        [PermissaoFilter("vendas_view")]
        public ActionResult Index(int page = 1, int pageSize = PagingDefaults.PageSize, string search = null)
        {
            try
            {
                return View(_vendaService.ListarPaginado(EmpresaId, page, pageSize, search));
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "VendaController.Index");
                ViewBag.Erro = "Erro ao carregar vendas.";
                return View(PagedResult<Models.Venda>.Empty(pageSize));
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
                                   int[] produtoIds, int[] quantidades, string[] precos)
        {
            try
            {
                if (produtoIds == null || quantidades == null || precos == null
                    || produtoIds.Length == 0
                    || produtoIds.Length != quantidades.Length
                    || produtoIds.Length != precos.Length)
                {
                    return ErroCreate("Dados da venda incompletos ou inconsistentes.");
                }

                var itens = new List<Services.VendaItemDto>(produtoIds.Length);
                for (int i = 0; i < produtoIds.Length; i++)
                {
                    decimal.TryParse(precos[i], System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal preco);
                    itens.Add(new Services.VendaItemDto { ProdutoId = produtoIds[i], Quantidade = quantidades[i], Preco = preco });
                }

                var venda = _vendaService.Criar(EmpresaId, clienteId, UsuarioId, vencimento, itens);
                AppLogger.Audit("venda_criada vendaId={0} clienteId={1} total={2} itens={3}",
                                venda.VendaId, clienteId, venda.Total, itens.Count);
                return RedirectToAction("Nota", new { id = venda.VendaId });
            }
            catch (DomainException dex)
            {
                return ErroCreate(dex.Message);
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "VendaController.Create clienteId={0}", clienteId);
                return ErroCreate("Erro ao registrar venda. Tente novamente.");
            }
        }

        private ActionResult ErroCreate(string mensagem)
        {
            ViewBag.Erro     = mensagem;
            ViewBag.Clientes = _clienteService.ListarPorEmpresa(EmpresaId);
            ViewBag.Produtos = _produtoService.ListarAtivos(EmpresaId);
            return View();
        }

        [PermissaoFilter("vendas_edit")]
        public ActionResult Edit(int id)
        {
            var venda = _vendaService.BuscarComItens(id, EmpresaId);
            if (venda == null)
            {
                TempData["Erro"] = "Venda não encontrada.";
                return RedirectToAction("Index");
            }
            if (venda.Status == Infrastructure.VendaStatus.Cancelada)
            {
                TempData["Erro"] = "Venda cancelada não pode ser editada.";
                return RedirectToAction("Index");
            }
            if (venda.ContasReceber != null && venda.ContasReceber.Any(c => c.Pago))
            {
                TempData["Erro"] = "Venda com pagamento registrado não pode ser editada.";
                return RedirectToAction("Index");
            }
            ViewBag.Clientes = _clienteService.ListarPorEmpresa(EmpresaId);
            ViewBag.Produtos = _produtoService.ListarAtivos(EmpresaId);
            return View(venda);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissaoFilter("vendas_edit")]
        public ActionResult Edit(int vendaId, int clienteId, DateTime vencimento,
                                 int[] produtoIds, int[] quantidades, string[] precos)
        {
            try
            {
                if (produtoIds == null || quantidades == null || precos == null
                    || produtoIds.Length == 0
                    || produtoIds.Length != quantidades.Length
                    || produtoIds.Length != precos.Length)
                {
                    return ErroEdit(vendaId, "Dados da venda incompletos ou inconsistentes.");
                }

                var itens = new List<Services.VendaItemDto>(produtoIds.Length);
                for (int i = 0; i < produtoIds.Length; i++)
                {
                    decimal.TryParse(precos[i], System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal preco);
                    itens.Add(new Services.VendaItemDto { ProdutoId = produtoIds[i], Quantidade = quantidades[i], Preco = preco });
                }

                _vendaService.Editar(vendaId, EmpresaId, clienteId, vencimento, itens);
                AppLogger.Audit("venda_editada vendaId={0} clienteId={1} itens={2}",
                                vendaId, clienteId, itens.Count);
                TempData["Sucesso"] = "Venda atualizada com sucesso.";
                return RedirectToAction("Nota", new { id = vendaId });
            }
            catch (DomainException dex)
            {
                return ErroEdit(vendaId, dex.Message);
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "VendaController.Edit vendaId={0}", vendaId);
                return ErroEdit(vendaId, "Erro ao editar venda. Tente novamente.");
            }
        }

        private ActionResult ErroEdit(int vendaId, string mensagem)
        {
            var venda = _vendaService.BuscarComItens(vendaId, EmpresaId);
            if (venda == null)
            {
                TempData["Erro"] = mensagem;
                return RedirectToAction("Index");
            }
            ViewBag.Erro     = mensagem;
            ViewBag.Clientes = _clienteService.ListarPorEmpresa(EmpresaId);
            ViewBag.Produtos = _produtoService.ListarAtivos(EmpresaId);
            return View(venda);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissaoFilter("vendas_delete")]
        public ActionResult Cancelar(int vendaId)
        {
            try
            {
                if (_vendaService.Cancelar(vendaId, EmpresaId))
                    AppLogger.Audit("venda_cancelada vendaId={0}", vendaId);
                else
                    TempData["Erro"] = "Não foi possível cancelar a venda.";
                return RedirectToAction("Index");
            }
            catch (DomainException dex)
            {
                TempData["Erro"] = dex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "VendaController.Cancelar vendaId={0}", vendaId);
                TempData["Erro"] = "Erro ao cancelar venda.";
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
                if (_vendaService.RegistrarPagamento(contaId, EmpresaId))
                    AppLogger.Audit("pagamento_registrado contaId={0}", contaId);
                else
                    TempData["Erro"] = "Conta não encontrada ou já paga.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "VendaController.RegistrarPagamento contaId={0}", contaId);
                TempData["Erro"] = "Erro ao registrar pagamento.";
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
                AppLogger.Error(ex, "VendaController.Nota");
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
                AppLogger.Error(ex, "VendaController.Financeiro");
                return RedirectToAction("Index");
            }
        }
    }
}
