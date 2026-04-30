using System.Collections.Generic;
using System.Linq;
using ATLAS_ERP.Data;
using ATLAS_ERP.Helpers;
using ATLAS_ERP.Models;

namespace ATLAS_ERP.Services
{
    public class UsuarioService
    {
        private readonly AtlasContext _db;

        public UsuarioService(AtlasContext db) { _db = db; }

        public List<Usuario> ListarPorEmpresa(int empresaId)
            => _db.Usuarios.AsNoTracking()
                  .Where(u => u.EmpresaId == empresaId)
                  .OrderBy(u => u.Name)
                  .ToList();

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
