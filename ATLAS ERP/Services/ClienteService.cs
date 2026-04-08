using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using ATLAS_ERP.Data;
using ATLAS_ERP.Models;

namespace ATLAS_ERP.Services
{
    public class ClienteService
    {
        private readonly AtlasContext _db;

        public ClienteService(AtlasContext db) { _db = db; }

        public List<Cliente> ListarPorEmpresa(int empresaId)
            => _db.Clientes.AsNoTracking()
                  .Where(c => c.EmpresaId == empresaId)
                  .OrderBy(c => c.Nome)
                  .ToList();

        public Cliente BuscarPorId(int id, int empresaId)
            => _db.Clientes.AsNoTracking()
                  .FirstOrDefault(c => c.ClienteId == id && c.EmpresaId == empresaId);

        public void Criar(Cliente cliente)
        {
            _db.Clientes.Add(cliente);
            _db.SaveChanges();
        }

        public bool Editar(int clienteId, int empresaId, string nome, string documento,
                           string email, string telefone, string endereco,
                           decimal limiteCredito, bool ativo)
        {
            var c = _db.Clientes.FirstOrDefault(x => x.ClienteId == clienteId && x.EmpresaId == empresaId);
            if (c == null) return false;

            c.Nome          = nome;
            c.Documento     = documento;
            c.Email         = email;
            c.Telefone      = telefone;
            c.Endereco      = endereco;
            c.LimiteCredito = limiteCredito;
            c.Ativo         = ativo;
            _db.Entry(c).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return true;
        }

        public bool Excluir(int clienteId, int empresaId)
        {
            var c = _db.Clientes.FirstOrDefault(x => x.ClienteId == clienteId && x.EmpresaId == empresaId);
            if (c == null) return false;

            _db.Clientes.Remove(c);
            _db.SaveChanges();
            return true;
        }
    }
}
