using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ATLAS_ERP.Data;
using ATLAS_ERP.Infrastructure;
using ATLAS_ERP.Models;

namespace ATLAS_ERP.Services
{
    public class VendaItemDto
    {
        public int     ProdutoId  { get; set; }
        public int     Quantidade { get; set; }
        public decimal Preco      { get; set; }
    }

    public class VendaService
    {
        private readonly AtlasContext _db;

        public VendaService(AtlasContext db) { _db = db; }

        public List<Venda> ListarPorEmpresa(int empresaId)
            => _db.Vendas.AsNoTracking()
                  .Include(v => v.Cliente)
                  .Include(v => v.Usuario)
                  .Where(v => v.EmpresaId == empresaId)
                  .OrderByDescending(v => v.DataVenda)
                  .ToList();

        public Venda BuscarComItens(int vendaId, int empresaId)
            => _db.Vendas
                  .Include(v => v.Cliente)
                  .Include(v => v.Itens.Select(i => i.Produto))
                  .Include(v => v.ContasReceber)
                  .FirstOrDefault(v => v.VendaId == vendaId && v.EmpresaId == empresaId);

        public Venda Criar(int empresaId, int clienteId, int usuarioId,
                           DateTime vencimento, List<VendaItemDto> itens)
        {
            decimal total = 0;
            foreach (var item in itens)
                total += item.Quantidade * item.Preco;

            var venda = new Venda
            {
                EmpresaId = empresaId,
                ClienteId = clienteId,
                UsuarioId = usuarioId,
                DataVenda = DateTime.Now,
                Status    = VendaStatus.Concluida,
                Total     = total
            };

            _db.Vendas.Add(venda);
            _db.SaveChanges();

            foreach (var item in itens)
            {
                _db.VendaItens.Add(new VendaItem
                {
                    VendaId       = venda.VendaId,
                    ProdutoId     = item.ProdutoId,
                    Quantidade    = item.Quantidade,
                    PrecoUnitario = item.Preco,
                    Subtotal      = item.Quantidade * item.Preco
                });

                var produto = _db.Produtos.Find(item.ProdutoId);
                if (produto != null)
                {
                    produto.EstoqueAtual -= item.Quantidade;
                    _db.Entry(produto).State = EntityState.Modified;
                }
            }

            _db.ContasReceber.Add(new ContaReceber
            {
                VendaId        = venda.VendaId,
                Valor          = total,
                DataVencimento = vencimento,
                Pago           = false
            });

            _db.SaveChanges();
            return venda;
        }

        public bool RegistrarPagamento(int contaId, int empresaId)
        {
            var conta = _db.ContasReceber
                           .Include(c => c.Venda)
                           .FirstOrDefault(c => c.ContaReceberId == contaId && c.Venda.EmpresaId == empresaId);
            if (conta == null) return false;

            conta.Pago          = true;
            conta.DataPagamento = DateTime.Now;
            _db.Entry(conta).State = EntityState.Modified;
            _db.SaveChanges();
            return true;
        }

        public bool Cancelar(int vendaId, int empresaId)
        {
            var venda = _db.Vendas
                           .Include(v => v.Itens)
                           .FirstOrDefault(v => v.VendaId == vendaId && v.EmpresaId == empresaId);
            if (venda == null) return false;

            venda.Status = VendaStatus.Cancelada;

            foreach (var item in venda.Itens)
            {
                var produto = _db.Produtos.Find(item.ProdutoId);
                if (produto != null)
                {
                    produto.EstoqueAtual += item.Quantidade;
                    _db.Entry(produto).State = EntityState.Modified;
                }
            }

            _db.Entry(venda).State = EntityState.Modified;
            _db.SaveChanges();
            return true;
        }
    }
}
