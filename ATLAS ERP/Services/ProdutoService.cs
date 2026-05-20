using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ATLAS_ERP.Data;
using ATLAS_ERP.Infrastructure;
using ATLAS_ERP.Models;

namespace ATLAS_ERP.Services
{
    public class ProdutoService
    {
        private readonly AtlasContext _db;

        public ProdutoService(AtlasContext db) { _db = db; }

        public List<Produto> ListarPorEmpresa(int empresaId)
            => _db.Produtos.AsNoTracking()
                  .Where(p => p.EmpresaId == empresaId)
                  .OrderBy(p => p.Nome)
                  .ToList();

        public PagedResult<Produto> ListarPaginado(int empresaId, int page, int pageSize, string search = null)
        {
            var q = _db.Produtos.AsNoTracking()
                .Where(p => p.EmpresaId == empresaId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                q = q.Where(p => p.Nome.Contains(term)
                              || (p.Categoria != null && p.Categoria.Contains(term)));
            }

            return q.OrderBy(p => p.Nome)
                    .ThenBy(p => p.ProdutoId)
                    .ToPagedResult(page, pageSize, search);
        }

        public List<Produto> ListarAtivos(int empresaId)
            => _db.Produtos.AsNoTracking()
                  .Where(p => p.EmpresaId == empresaId && p.Ativo)
                  .OrderBy(p => p.Nome)
                  .ToList();

        public Produto BuscarPorId(int id, int empresaId)
            => _db.Produtos.AsNoTracking()
                  .FirstOrDefault(p => p.ProdutoId == id && p.EmpresaId == empresaId);

        public void Criar(Produto produto)
        {
            _db.Produtos.Add(produto);
            _db.SaveChanges();
        }

        public bool Editar(int produtoId, int empresaId, string nome, string categoria,
                           decimal precoVenda, int estoqueMinimo, bool ativo, string fotoUrl = null)
        {
            var p = _db.Produtos.FirstOrDefault(x => x.ProdutoId == produtoId && x.EmpresaId == empresaId);
            if (p == null) return false;

            p.Nome          = nome;
            p.Categoria     = categoria;
            p.PrecoVenda    = precoVenda;
            p.EstoqueMinimo = estoqueMinimo;
            p.Ativo         = ativo;
            if (fotoUrl != null) p.FotoUrl = fotoUrl;
            _db.Entry(p).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return true;
        }

        public bool Excluir(int produtoId, int empresaId)
        {
            var p = _db.Produtos.FirstOrDefault(x => x.ProdutoId == produtoId && x.EmpresaId == empresaId);
            if (p == null) return false;

            _db.Produtos.Remove(p);
            _db.SaveChanges();
            return true;
        }

        public bool AtualizarEstoque(int produtoId, int quantidade)
        {
            var p = _db.Produtos.Find(produtoId);
            if (p == null) return false;

            p.EstoqueAtual -= quantidade;
            _db.Entry(p).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return true;
        }
    }
}
