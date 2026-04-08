using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ATLAS_ERP.Data;
using ATLAS_ERP.Infrastructure;
using ATLAS_ERP.Models;

namespace ATLAS_ERP.Services
{
    public class ResumoFinanceiro
    {
        public decimal ReceitaTotal    { get; set; }
        public decimal ReceitaMes      { get; set; }
        public decimal ContasPendentes { get; set; }
        public decimal ContasRecebidas { get; set; }
        public int     TotalVendas     { get; set; }
        public int     VendasMes       { get; set; }
        public List<TopProduto>   TopProdutos   { get; set; }
        public List<VendaResumo>  UltimasVendas { get; set; }
        public List<ContaReceber> ContasVencer  { get; set; }
    }

    public class TopProduto
    {
        public string  Nome           { get; set; }
        public int     TotalVendido   { get; set; }
        public decimal ReceitaGerada  { get; set; }
    }

    public class VendaResumo
    {
        public int      VendaId   { get; set; }
        public string   Cliente   { get; set; }
        public DateTime DataVenda { get; set; }
        public decimal  Total     { get; set; }
        public string   Status    { get; set; }
    }

    public class RelatorioService
    {
        private readonly AtlasContext _db;

        public RelatorioService(AtlasContext db) { _db = db; }

        public ResumoFinanceiro GerarResumo(int empresaId)
        {
            var agora     = DateTime.Now;
            var inicioMes = new DateTime(agora.Year, agora.Month, 1);

            var vendas = _db.Vendas.AsNoTracking()
                            .Include(v => v.Cliente)
                            .Where(v => v.EmpresaId == empresaId && v.Status != VendaStatus.Cancelada)
                            .ToList();

            var contas = _db.ContasReceber.AsNoTracking()
                            .Include(c => c.Venda)
                            .Where(c => c.Venda.EmpresaId == empresaId)
                            .ToList();

            // busca itens em memória para evitar limitações do EF6 com GroupBy
            var itensRaw = _db.VendaItens.AsNoTracking()
                              .Include(i => i.Produto)
                              .Include(i => i.Venda)
                              .Where(i => i.Venda.EmpresaId == empresaId && i.Venda.Status != VendaStatus.Cancelada)
                              .ToList();

            var topProdutos = itensRaw
                .GroupBy(i => new { i.ProdutoId, Nome = i.Produto != null ? i.Produto.Nome : "—" })
                .Select(g => new TopProduto
                {
                    Nome          = g.Key.Nome,
                    TotalVendido  = g.Sum(i => i.Quantidade),
                    ReceitaGerada = g.Sum(i => i.Subtotal)
                })
                .OrderByDescending(t => t.ReceitaGerada)
                .Take(5)
                .ToList();

            var contasVencer = contas
                .Where(c => !c.Pago && c.DataVencimento >= agora.Date && c.DataVencimento <= agora.AddDays(30))
                .OrderBy(c => c.DataVencimento)
                .ToList();

            var ultimasVendas = vendas
                .OrderByDescending(v => v.DataVenda)
                .Take(10)
                .Select(v => new VendaResumo
                {
                    VendaId   = v.VendaId,
                    Cliente   = v.Cliente != null ? v.Cliente.Nome : "—",
                    DataVenda = v.DataVenda,
                    Total     = v.Total,
                    Status    = v.Status
                })
                .ToList();

            return new ResumoFinanceiro
            {
                ReceitaTotal    = vendas.Sum(v => v.Total),
                ReceitaMes      = vendas.Where(v => v.DataVenda >= inicioMes).Sum(v => v.Total),
                ContasPendentes = contas.Where(c => !c.Pago).Sum(c => c.Valor),
                ContasRecebidas = contas.Where(c => c.Pago).Sum(c => c.Valor),
                TotalVendas     = vendas.Count,
                VendasMes       = vendas.Count(v => v.DataVenda >= inicioMes),
                TopProdutos     = topProdutos,
                UltimasVendas   = ultimasVendas,
                ContasVencer    = contasVencer
            };
        }
    }
}
