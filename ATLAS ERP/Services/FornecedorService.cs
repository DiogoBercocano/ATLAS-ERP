using System.Collections.Generic;
using System.Linq;
using ATLAS_ERP.Data;
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

        public Fornecedor BuscarPorId(int id, int empresaId)
            => _db.Fornecedores.AsNoTracking()
                  .FirstOrDefault(f => f.FornecedorId == id && f.EmpresaId == empresaId);

        public void Criar(Fornecedor fornecedor)
        {
            _db.Fornecedores.Add(fornecedor);
            _db.SaveChanges();
        }

        public bool Editar(int fornecedorId, int empresaId, string nome, string cnpj,
                           string telefone, string email)
        {
            var f = _db.Fornecedores.FirstOrDefault(x => x.FornecedorId == fornecedorId && x.EmpresaId == empresaId);
            if (f == null) return false;

            f.Nome     = nome;
            f.CNPJ     = cnpj;
            f.Telefone = telefone;
            f.Email    = email;
            _db.Entry(f).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return true;
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
