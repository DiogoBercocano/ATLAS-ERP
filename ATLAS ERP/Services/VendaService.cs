using System;
using System.Collections.Generic;
using System.Data;
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

        public PagedResult<Venda> ListarPaginado(int empresaId, int page, int pageSize, string search = null)
        {
            var q = _db.Vendas.AsNoTracking()
                .Include(v => v.Cliente)
                .Include(v => v.Usuario)
                .Where(v => v.EmpresaId == empresaId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                q = q.Where(v => (v.Cliente != null && v.Cliente.Nome.Contains(term))
                              || (v.Status != null && v.Status.Contains(term)));
            }

            return q.OrderByDescending(v => v.DataVenda)
                    .ThenByDescending(v => v.VendaId)
                    .ToPagedResult(page, pageSize, search);
        }

        public Venda BuscarComItens(int vendaId, int empresaId)
            => _db.Vendas
                  .Include(v => v.Cliente)
                  .Include(v => v.Itens.Select(i => i.Produto))
                  .Include(v => v.ContasReceber)
                  .FirstOrDefault(v => v.VendaId == vendaId && v.EmpresaId == empresaId);

        public Venda Criar(int empresaId, int clienteId, int usuarioId,
                           DateTime vencimento, List<VendaItemDto> itens)
        {
            ValidarEntradaCriacao(itens, vencimento);

            using (var transaction = _db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    var clienteValido = _db.Clientes
                        .AsNoTracking()
                        .Any(c => c.ClienteId == clienteId && c.EmpresaId == empresaId && c.Ativo);
                    if (!clienteValido)
                        throw new DomainException("Cliente inválido ou inativo para a empresa.");

                    var quantidadesPorProduto = itens
                        .GroupBy(i => i.ProdutoId)
                        .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantidade));
                    var produtoIds = quantidadesPorProduto.Keys.ToList();

                    var produtos = LockProdutosParaUpdate(empresaId, produtoIds);

                    if (produtos.Count != produtoIds.Count)
                        throw new DomainException("Um ou mais produtos não pertencem à empresa.");

                    foreach (var kv in quantidadesPorProduto)
                    {
                        var produto = produtos[kv.Key];
                        if (!produto.Ativo)
                            throw new DomainException("Produto '" + produto.Nome + "' está inativo.");
                        if (produto.EstoqueAtual < kv.Value)
                            throw new DomainException(
                                "Estoque insuficiente para '" + produto.Nome +
                                "'. Disponível: " + produto.EstoqueAtual +
                                ", solicitado: " + kv.Value + ".");
                        produto.EstoqueAtual -= kv.Value;
                    }

                    decimal total = 0m;
                    var vendaItens = new List<VendaItem>(itens.Count);
                    foreach (var item in itens)
                    {
                        var subtotal = item.Quantidade * item.Preco;
                        total += subtotal;
                        vendaItens.Add(new VendaItem
                        {
                            ProdutoId     = item.ProdutoId,
                            Quantidade    = item.Quantidade,
                            PrecoUnitario = item.Preco,
                            Subtotal      = subtotal
                        });
                    }

                    var venda = new Venda
                    {
                        EmpresaId = empresaId,
                        ClienteId = clienteId,
                        UsuarioId = usuarioId,
                        DataVenda = DateTime.Now,
                        Status    = VendaStatus.Concluida,
                        Total     = total,
                        Itens     = vendaItens,
                        ContasReceber = new List<ContaReceber>
                        {
                            new ContaReceber
                            {
                                Valor          = total,
                                DataVencimento = vencimento,
                                Pago           = false
                            }
                        }
                    };

                    _db.Vendas.Add(venda);
                    _db.SaveChanges();
                    transaction.Commit();
                    return venda;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// SELECT WITH (UPDLOCK, ROWLOCK) — adquire lock exclusivo de update
        /// nas linhas dos produtos até o COMMIT/ROLLBACK da transação atual.
        /// Previne race condition em vendas concorrentes do mesmo produto.
        /// IDs são integers garantidos pelo model binding (sem risco de injection).
        /// </summary>
        private Dictionary<int, Produto> LockProdutosParaUpdate(int empresaId, List<int> produtoIds)
        {
            if (produtoIds == null || produtoIds.Count == 0)
                return new Dictionary<int, Produto>();

            var idsCsv = string.Join(",", produtoIds.Select(id => id.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            var sql = "SELECT * FROM Produtoes WITH (UPDLOCK, ROWLOCK) " +
                      "WHERE EmpresaId = @p0 AND ProdutoId IN (" + idsCsv + ")";

            return _db.Produtos.SqlQuery(sql, empresaId).ToDictionary(p => p.ProdutoId);
        }

        public Venda Editar(int vendaId, int empresaId, int clienteId,
                            DateTime vencimento, List<VendaItemDto> novosItens)
        {
            ValidarEntradaCriacao(novosItens, vencimento);

            using (var transaction = _db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    var venda = _db.Vendas
                        .Include(v => v.Itens)
                        .Include(v => v.ContasReceber)
                        .FirstOrDefault(v => v.VendaId == vendaId && v.EmpresaId == empresaId);
                    if (venda == null)
                        throw new DomainException("Venda não encontrada.");
                    if (venda.Status == VendaStatus.Cancelada)
                        throw new DomainException("Venda cancelada não pode ser editada.");
                    if (venda.ContasReceber != null && venda.ContasReceber.Any(c => c.Pago))
                        throw new DomainException("Venda com pagamento já registrado não pode ser editada.");

                    var clienteValido = _db.Clientes
                        .AsNoTracking()
                        .Any(c => c.ClienteId == clienteId && c.EmpresaId == empresaId && c.Ativo);
                    if (!clienteValido)
                        throw new DomainException("Cliente inválido ou inativo para a empresa.");

                    var antigos = venda.Itens
                        .GroupBy(i => i.ProdutoId)
                        .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantidade));
                    var novos = novosItens
                        .GroupBy(i => i.ProdutoId)
                        .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantidade));

                    var todosIds = antigos.Keys.Union(novos.Keys).Distinct().ToList();

                    var produtos = LockProdutosParaUpdate(empresaId, todosIds);
                    if (produtos.Count != todosIds.Count)
                        throw new DomainException("Um ou mais produtos não pertencem à empresa.");

                    foreach (var pid in todosIds)
                    {
                        var qtAntiga = antigos.TryGetValue(pid, out var a) ? a : 0;
                        var qtNova   = novos.TryGetValue(pid, out var n)   ? n : 0;
                        var delta    = qtNova - qtAntiga;
                        var produto  = produtos[pid];

                        if (delta > 0)
                        {
                            if (!produto.Ativo)
                                throw new DomainException("Produto '" + produto.Nome + "' está inativo.");
                            if (produto.EstoqueAtual < delta)
                                throw new DomainException(
                                    "Estoque insuficiente para '" + produto.Nome +
                                    "'. Disponível: " + produto.EstoqueAtual +
                                    ", adicional solicitado: " + delta + ".");
                        }
                        produto.EstoqueAtual -= delta;
                    }

                    var itensAntigos = venda.Itens.ToList();
                    foreach (var antigo in itensAntigos)
                        _db.VendaItens.Remove(antigo);

                    decimal novoTotal = 0m;
                    foreach (var item in novosItens)
                    {
                        var subtotal = item.Quantidade * item.Preco;
                        novoTotal += subtotal;
                        _db.VendaItens.Add(new VendaItem
                        {
                            VendaId       = vendaId,
                            ProdutoId     = item.ProdutoId,
                            Quantidade    = item.Quantidade,
                            PrecoUnitario = item.Preco,
                            Subtotal      = subtotal
                        });
                    }

                    venda.ClienteId = clienteId;
                    venda.Total     = novoTotal;

                    var conta = venda.ContasReceber?.FirstOrDefault(c => !c.Pago);
                    if (conta != null)
                    {
                        conta.Valor          = novoTotal;
                        conta.DataVencimento = vencimento;
                    }

                    _db.SaveChanges();
                    transaction.Commit();
                    return venda;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public bool RegistrarPagamento(int contaId, int empresaId)
        {
            var conta = _db.ContasReceber
                           .Include(c => c.Venda)
                           .FirstOrDefault(c => c.ContaReceberId == contaId && c.Venda.EmpresaId == empresaId);
            if (conta == null) return false;
            if (conta.Pago) return false;

            conta.Pago          = true;
            conta.DataPagamento = DateTime.Now;
            _db.SaveChanges();
            return true;
        }

        public bool Cancelar(int vendaId, int empresaId)
        {
            using (var transaction = _db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    var venda = _db.Vendas
                                   .Include(v => v.Itens)
                                   .FirstOrDefault(v => v.VendaId == vendaId && v.EmpresaId == empresaId);
                    if (venda == null) return false;
                    if (venda.Status == VendaStatus.Cancelada) return false;

                    venda.Status = VendaStatus.Cancelada;

                    var quantidadesPorProduto = venda.Itens
                        .GroupBy(i => i.ProdutoId)
                        .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantidade));
                    var produtoIds = quantidadesPorProduto.Keys.ToList();

                    var produtos = LockProdutosParaUpdate(empresaId, produtoIds);

                    foreach (var kv in quantidadesPorProduto)
                    {
                        if (produtos.TryGetValue(kv.Key, out var produto))
                            produto.EstoqueAtual += kv.Value;
                    }

                    _db.SaveChanges();
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private static void ValidarEntradaCriacao(List<VendaItemDto> itens, DateTime vencimento)
        {
            if (itens == null || itens.Count == 0)
                throw new DomainException("A venda deve conter ao menos um item.");

            if (vencimento.Date < DateTime.Today)
                throw new DomainException("Data de vencimento não pode ser anterior a hoje.");

            foreach (var item in itens)
            {
                if (item.Quantidade <= 0)
                    throw new DomainException("Quantidade deve ser maior que zero.");
                if (item.Preco < 0m)
                    throw new DomainException("Preço unitário não pode ser negativo.");
            }
        }
    }
}
