using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ATLAS_ERP.Data;
using ATLAS_ERP.Helpers;
using ATLAS_ERP.Infrastructure;
using ATLAS_ERP.Models;

namespace ATLAS_ERP.Services
{
    public class UsuarioService
    {
        private readonly AtlasContext _db;

        public UsuarioService(AtlasContext db) { _db = db; }

        public List<Usuario> ListarPorEmpresa(int empresaId)
            => _db.Usuarios.AsNoTracking()
                  .Include(u => u.Cargo)
                  .Where(u => u.EmpresaId == empresaId)
                  .OrderBy(u => u.Name)
                  .ToList();

        public PagedResult<Usuario> ListarPaginado(int empresaId, int page, int pageSize, string search = null)
        {
            var q = _db.Usuarios.AsNoTracking()
                .Include(u => u.Cargo)
                .Where(u => u.EmpresaId == empresaId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                q = q.Where(u => u.Name.Contains(term)
                              || (u.Email != null && u.Email.Contains(term)));
            }

            return q.OrderBy(u => u.Name)
                    .ThenBy(u => u.UsuarioId)
                    .ToPagedResult(page, pageSize, search);
        }

        public Usuario BuscarPorId(int id, int empresaId)
            => _db.Usuarios.AsNoTracking()
                  .FirstOrDefault(u => u.UsuarioId == id && u.EmpresaId == empresaId);

        public void Criar(Usuario usuario)
        {
            usuario.SenhaHash = PasswordHelper.HashSenha(usuario.SenhaHash);
            usuario.Ativo = true;
            _db.Usuarios.Add(usuario);
            _db.SaveChanges();
        }

        public bool Editar(int usuarioId, int empresaId, string name, string email, int? cargoId, bool ativo)
        {
            var u = _db.Usuarios.FirstOrDefault(x => x.UsuarioId == usuarioId && x.EmpresaId == empresaId);
            if (u == null) return false;

            u.Name  = name;
            u.Email = email;
            u.CargoId = cargoId;
            u.Ativo = ativo;
            _db.Entry(u).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return true;
        }

        public bool Excluir(int usuarioId, int empresaId)
        {
            var u = _db.Usuarios.FirstOrDefault(x => x.UsuarioId == usuarioId && x.EmpresaId == empresaId);
            if (u == null) return false;

            _db.Usuarios.Remove(u);
            _db.SaveChanges();
            return true;
        }
    }
}
