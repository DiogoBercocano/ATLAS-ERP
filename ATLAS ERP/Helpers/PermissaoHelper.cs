using System.Collections.Generic;
using System.Linq;
using System.Web;
using ATLAS_ERP.Infrastructure;

namespace ATLAS_ERP.Helpers
{
    public static class PermissaoHelper
    {
        /// <summary>
        /// Verifica se o usuário da sessão tem uma permissão específica.
        /// Backed by PermissaoCache (Session + global version) — sem queries em hot path.
        /// </summary>
        public static bool UsuarioTemPermissao(string chavePermissao)
        {
            var cargoId = ObterCargoId();
            if (!cargoId.HasValue) return false;
            return PermissaoCache.TemPermissao(cargoId.Value, chavePermissao);
        }

        /// <summary>
        /// Retorna a lista de permissões do usuário (cacheada).
        /// </summary>
        public static List<string> ObterPermissoesUsuario()
        {
            var cargoId = ObterCargoId();
            if (!cargoId.HasValue) return new List<string>();
            return PermissaoCache.ObterPermissoes(cargoId.Value).ToList();
        }

        private static int? ObterCargoId()
        {
            var session = HttpContext.Current?.Session;
            if (session == null || session["CargoId"] == null) return null;

            if (session["CargoId"] is int direto) return direto;
            if (int.TryParse(session["CargoId"].ToString(), out int parsed)) return parsed;
            return null;
        }
    }
}
