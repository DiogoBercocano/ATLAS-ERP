using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using ATLAS_ERP.Data;
using ATLAS_ERP.Infrastructure;
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

        public PagedResult<Cliente> ListarPaginado(int empresaId, int page, int pageSize, string search = null)
        {
            var q = _db.Clientes.AsNoTracking()
                .Where(c => c.EmpresaId == empresaId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                q = q.Where(c => c.Nome.Contains(term)
                              || (c.Documento != null && c.Documento.Contains(term))
                              || (c.Email != null && c.Email.Contains(term))
                              || (c.Telefone != null && c.Telefone.Contains(term)));
            }

            return q.OrderBy(c => c.Nome)
                    .ThenBy(c => c.ClienteId)
                    .ToPagedResult(page, pageSize, search);
        }

        public Cliente BuscarPorId(int id, int empresaId)
            => _db.Clientes.AsNoTracking()
                  .FirstOrDefault(c => c.ClienteId == id && c.EmpresaId == empresaId);

        public void Criar(Cliente cliente)
        {
            cliente.Documento = ValidarENormalizarDocumento(cliente.Documento);
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
            c.Documento     = ValidarENormalizarDocumento(documento);
            c.Email         = email;
            c.Telefone      = telefone;
            c.Endereco      = endereco;
            c.LimiteCredito = limiteCredito;
            c.Ativo         = ativo;
            _db.Entry(c).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return true;
        }

        private static string ValidarENormalizarDocumento(string documento)
        {
            if (string.IsNullOrWhiteSpace(documento)) return null;
            var normalizado = DocumentoValidator.Normalizar(documento);
            if (normalizado.Length == 11)
            {
                if (!DocumentoValidator.ValidarCPF(normalizado))
                    throw new DomainException("CPF inválido.");
                return normalizado;
            }
            if (normalizado.Length == 14)
            {
                if (!DocumentoValidator.ValidarCNPJ(normalizado))
                    throw new DomainException("CNPJ inválido.");
                return normalizado;
            }
            throw new DomainException("Documento deve ser CPF (11 dígitos) ou CNPJ (14 dígitos).");
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
