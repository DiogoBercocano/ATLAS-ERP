using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ATLAS_ERP.Data;
using ATLAS_ERP.Infrastructure;
using ATLAS_ERP.Models;

namespace ATLAS_ERP.Services
{
    public class FornecedorService
    {
        private readonly AtlasContext _db;

        public FornecedorService(AtlasContext db) { _db = db; }

        public List<Fornecedor> ListarPorEmpresa(int empresaId)
            => _db.Fornecedores.AsNoTracking()
                  .Where(f => f.EmpresaId == empresaId)
                  .OrderBy(f => f.Nome)
                  .ToList();

        public PagedResult<Fornecedor> ListarPaginado(int empresaId, int page, int pageSize, string search = null)
        {
            var q = _db.Fornecedores.AsNoTracking()
                .Where(f => f.EmpresaId == empresaId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                q = q.Where(f => f.Nome.Contains(term)
                              || (f.CNPJ != null && f.CNPJ.Contains(term))
                              || (f.Email != null && f.Email.Contains(term))
                              || (f.Telefone != null && f.Telefone.Contains(term)));
            }

            return q.OrderBy(f => f.Nome)
                    .ThenBy(f => f.FornecedorId)
                    .ToPagedResult(page, pageSize, search);
        }

        public Fornecedor BuscarPorId(int id, int empresaId)
            => _db.Fornecedores.AsNoTracking()
                  .FirstOrDefault(f => f.FornecedorId == id && f.EmpresaId == empresaId);

        public void Criar(Fornecedor fornecedor)
        {
            fornecedor.CNPJ = ValidarENormalizarCnpj(fornecedor.CNPJ);
            _db.Fornecedores.Add(fornecedor);
            _db.SaveChanges();
        }

        public bool Editar(int fornecedorId, int empresaId, string nome, string cnpj,
                           string telefone, string email)
        {
            var f = _db.Fornecedores.FirstOrDefault(x => x.FornecedorId == fornecedorId && x.EmpresaId == empresaId);
            if (f == null) return false;

            f.Nome     = nome;
            f.CNPJ     = ValidarENormalizarCnpj(cnpj);
            f.Telefone = telefone;
            f.Email    = email;
            _db.Entry(f).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return true;
        }

        private static string ValidarENormalizarCnpj(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                throw new DomainException("CNPJ é obrigatório para fornecedor.");
            var normalizado = DocumentoValidator.Normalizar(cnpj);
            if (normalizado.Length != 14)
                throw new DomainException("CNPJ deve ter 14 dígitos.");
            if (!DocumentoValidator.ValidarCNPJ(normalizado))
                throw new DomainException("CNPJ inválido.");
            return normalizado;
        }

        public bool Excluir(int fornecedorId, int empresaId)
        {
            var f = _db.Fornecedores.FirstOrDefault(x => x.FornecedorId == fornecedorId && x.EmpresaId == empresaId);
            if (f == null) return false;

            _db.Fornecedores.Remove(f);
            _db.SaveChanges();
            return true;
        }
    }
}
