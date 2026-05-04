using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ATLAS_ERP.Data;

namespace ATLAS_ERP.Helpers
{
    public static class PermissaoHelper
    {
        /// <summary>
        /// Verifica se o usuário da sessão tem uma permissão específica
        /// </summary>
        public static bool UsuarioTemPermissao(string chavePermissao)
        {
            var session = HttpContext.Current?.Session;

            if (session == null || session["CargoId"] == null)
                return false;

            if (!int.TryParse(session["CargoId"].ToString(), out int cargoId))
                return false;

            try
            {
                using (var db = new AtlasContext())
                {
                    return db.CargoPermissoes
                        .Where(cp => cp.CargoId == cargoId)
                        .Any(cp => cp.Permissao.Chave == chavePermissao);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Retorna a lista de permissões do usuário
        /// </summary>
        public static List<string> ObterPermissoesUsuario()
        {
            var session = HttpContext.Current?.Session;

            if (session == null || session["CargoId"] == null)
                return new List<string>();

            if (!int.TryParse(session["CargoId"].ToString(), out int cargoId))
                return new List<string>();

            try
            {
                using (var db = new AtlasContext())
                {
                    return db.CargoPermissoes
                        .Where(cp => cp.CargoId == cargoId)
                        .Select(cp => cp.Permissao.Chave)
                        .ToList();
                }
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
