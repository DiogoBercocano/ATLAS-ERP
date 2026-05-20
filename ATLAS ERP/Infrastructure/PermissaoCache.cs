using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Web;
using ATLAS_ERP.Data;

namespace ATLAS_ERP.Infrastructure
{
    /// <summary>
    /// Cache de permissões por sessão com invalidação por versão global.
    ///
    /// Mecânica:
    ///   - Static long _globalVersion (Interlocked.Increment) — escopo de AppDomain
    ///   - Cada Session armazena HashSet<string> + a versão em que foi populada
    ///   - Cache miss OU mismatch de versão → recarrega do banco
    ///
    /// Invalidação:
    ///   - InvalidarGlobal()  — alterações em CargoPermissoes (afeta todos os usuários do cargo)
    ///   - InvalidarSessao()  — alteração de cargo do próprio usuário ou logout
    /// </summary>
    public static class PermissaoCache
    {
        private const string SessionKeyPermissoes = "PermissoesSet";
        private const string SessionKeyVersao     = "PermissoesSetVersao";

        private static long _globalVersion;

        public static long GlobalVersion => Interlocked.Read(ref _globalVersion);

        public static void InvalidarGlobal()
        {
            Interlocked.Increment(ref _globalVersion);
        }

        public static void InvalidarSessao(HttpSessionStateBase session)
        {
            if (session == null) return;
            session.Remove(SessionKeyPermissoes);
            session.Remove(SessionKeyVersao);
        }

        public static void InvalidarSessao()
        {
            if (HttpContext.Current?.Session == null) return;
            HttpContext.Current.Session.Remove(SessionKeyPermissoes);
            HttpContext.Current.Session.Remove(SessionKeyVersao);
        }

        public static HashSet<string> ObterPermissoes(int cargoId)
        {
            var session = HttpContext.Current?.Session;
            if (session == null) return CarregarDoBanco(cargoId);

            var versaoAtual = GlobalVersion;
            var cached      = session[SessionKeyPermissoes] as HashSet<string>;
            var versaoCache = session[SessionKeyVersao]     as long?;

            if (cached != null && versaoCache.HasValue && versaoCache.Value == versaoAtual)
                return cached;

            var fresh = CarregarDoBanco(cargoId);
            session[SessionKeyPermissoes] = fresh;
            session[SessionKeyVersao]     = versaoAtual;
            return fresh;
        }

        public static bool TemPermissao(int cargoId, string chave)
        {
            if (string.IsNullOrEmpty(chave)) return false;
            return ObterPermissoes(cargoId).Contains(chave);
        }

        public static bool TemAlguma(int cargoId, params string[] chaves)
        {
            if (chaves == null || chaves.Length == 0) return false;
            var perms = ObterPermissoes(cargoId);
            for (int i = 0; i < chaves.Length; i++)
                if (perms.Contains(chaves[i])) return true;
            return false;
        }

        private static HashSet<string> CarregarDoBanco(int cargoId)
        {
            try
            {
                using (var db = new AtlasContext())
                {
                    var lista = db.CargoPermissoes
                        .AsNoTracking()
                        .Where(cp => cp.CargoId == cargoId)
                        .Select(cp => cp.Permissao.Chave)
                        .ToList();
                    return new HashSet<string>(lista, StringComparer.Ordinal);
                }
            }
            catch
            {
                return new HashSet<string>(StringComparer.Ordinal);
            }
        }
    }
}
