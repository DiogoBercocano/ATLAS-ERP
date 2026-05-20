using System;
using System.Web;
using System.Web.Mvc;
using ATLAS_ERP.Infrastructure;

namespace ATLAS_ERP.Filters
{
    /// <summary>
    /// HTTPS enforcement com redirect 301 (permanente) para GETs e bloqueio explícito
    /// de POST sob HTTP — comportamento alinhado com OWASP TLS Cheat Sheet.
    ///
    /// Difere do <see cref="RequireHttpsAttribute"/> padrão do MVC:
    ///   - usa 301 (permanente) ao invés de 302 (temporário), reforçando HSTS
    ///   - audita tentativas de POST em texto puro (potencial captura de credenciais)
    ///   - respeita o header X-Forwarded-Proto quando atrás de proxy reverso (Cloudflare,
    ///     NGINX, ALB) — corrigindo loop de redirect quando o TLS termina no edge
    /// </summary>
    public sealed class RequireHttpsRedirectAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null) throw new ArgumentNullException(nameof(filterContext));

            var request = filterContext.HttpContext.Request;
            if (IsSecure(request)) return;

            if (string.Equals(request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                AppLogger.Audit("https_violacao_post path={0} ip={1}",
                                request.RawUrl, request.UserHostAddress);

                filterContext.Result = new HttpStatusCodeResult(403,
                    "POST sob HTTP não é permitido. Use HTTPS.");
                return;
            }

            var url = "https://" + request.Url.Authority + request.RawUrl;
            filterContext.Result = new RedirectResult(url, permanent: true);
        }

        private static bool IsSecure(HttpRequestBase request)
        {
            if (request.IsSecureConnection) return true;

            // Atrás de proxy reverso (Cloudflare, NGINX, ALB) o TLS termina no edge e a
            // request chega ao IIS como HTTP. O header indica que o cliente original usou HTTPS.
            var forwardedProto = request.Headers["X-Forwarded-Proto"];
            return !string.IsNullOrEmpty(forwardedProto) &&
                   string.Equals(forwardedProto, "https", StringComparison.OrdinalIgnoreCase);
        }
    }
}
