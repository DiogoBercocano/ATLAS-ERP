using System;
using System.Configuration;
using System.Web;

namespace ATLAS_ERP.Infrastructure
{
    /// <summary>
    /// Injeta cabeçalhos HTTP de segurança em todas as respostas e remove cabeçalhos
    /// que vazam informações de versão do servidor.
    ///
    /// Cabeçalhos adicionados:
    ///   - Content-Security-Policy        (mitiga XSS / data injection)
    ///   - X-Content-Type-Options         (impede MIME sniffing)
    ///   - X-Frame-Options                (impede clickjacking — legado, ainda exigido por scanners)
    ///   - Referrer-Policy                (controla vazamento via Referer)
    ///   - Permissions-Policy             (desabilita APIs sensíveis do navegador)
    ///   - Strict-Transport-Security      (HSTS — apenas sob HTTPS)
    ///   - Cross-Origin-Opener-Policy     (isolamento de janelas)
    ///   - Cross-Origin-Resource-Policy   (isolamento de recursos)
    ///
    /// Cabeçalhos removidos:
    ///   - Server
    ///   - X-AspNet-Version
    ///   - X-AspNetMvc-Version
    ///   - X-Powered-By (também removível via Web.config)
    ///
    /// Toda a configuração é externa via appSettings — alterar política não exige recompilar.
    /// Valores padrão são seguros para a stack atual (Bootstrap, jQuery, inline-styles parciais).
    /// </summary>
    public sealed class SecurityHeadersModule : IHttpModule
    {
        // Cache de configuração (lida uma única vez por AppDomain — alterações em Web.config
        // disparam recycle do app, então não há risco de cache stale).
        private static readonly SecurityOptions _options = SecurityOptions.LoadFromAppSettings();

        public void Init(HttpApplication context)
        {
            context.PreSendRequestHeaders += OnPreSendRequestHeaders;
        }

        public void Dispose() { }

        private static void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            if (app == null) return;

            var response = app.Context?.Response;
            var request  = app.Context?.Request;
            if (response == null || request == null) return;

            try
            {
                ApplyHeaders(request, response);
                RemoveLeakHeaders(response);
            }
            catch (Exception ex)
            {
                // Falha aqui não deve derrubar a request. Loga e segue.
                AppLogger.Error(ex, "security_headers_module_falhou");
            }
        }

        private static void ApplyHeaders(HttpRequest request, HttpResponse response)
        {
            SetIfMissing(response, "Content-Security-Policy",  _options.ContentSecurityPolicy);
            SetIfMissing(response, "X-Content-Type-Options",   "nosniff");
            SetIfMissing(response, "X-Frame-Options",          _options.XFrameOptions);
            SetIfMissing(response, "Referrer-Policy",          _options.ReferrerPolicy);
            SetIfMissing(response, "Permissions-Policy",       _options.PermissionsPolicy);
            SetIfMissing(response, "Cross-Origin-Opener-Policy",   _options.CrossOriginOpenerPolicy);
            SetIfMissing(response, "Cross-Origin-Resource-Policy", _options.CrossOriginResourcePolicy);

            // X-XSS-Protection desligado intencionalmente. O cabeçalho legado (1; mode=block)
            // introduzia vulnerabilidades em navegadores antigos e é ignorado pelos modernos
            // (Chromium / Firefox removeram o filtro). CSP cobre o caso.
            SetIfMissing(response, "X-XSS-Protection", "0");

            // HSTS apenas sob HTTPS — emitir sob HTTP é violação da RFC 6797.
            if (_options.EnableHsts && request.IsSecureConnection)
            {
                var value = "max-age=" + _options.HstsMaxAgeSeconds;
                if (_options.HstsIncludeSubDomains) value += "; includeSubDomains";
                if (_options.HstsPreload)           value += "; preload";
                SetIfMissing(response, "Strict-Transport-Security", value);
            }
        }

        private static void RemoveLeakHeaders(HttpResponse response)
        {
            if (!_options.RemoveServerHeader) return;

            // Server e X-Powered-By são removidos pelo IIS via Web.config quando suportado,
            // mas removê-los aqui garante cobertura em IIS Express / versões antigas.
            response.Headers.Remove("Server");
            response.Headers.Remove("X-AspNet-Version");
            response.Headers.Remove("X-AspNetMvc-Version");
            response.Headers.Remove("X-Powered-By");
        }

        private static void SetIfMissing(HttpResponse response, string name, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            if (string.IsNullOrEmpty(response.Headers[name]))
                response.Headers.Set(name, value);
        }

        private sealed class SecurityOptions
        {
            public string ContentSecurityPolicy     { get; private set; }
            public string XFrameOptions             { get; private set; }
            public string ReferrerPolicy            { get; private set; }
            public string PermissionsPolicy         { get; private set; }
            public string CrossOriginOpenerPolicy   { get; private set; }
            public string CrossOriginResourcePolicy { get; private set; }
            public bool   EnableHsts                { get; private set; }
            public int    HstsMaxAgeSeconds         { get; private set; }
            public bool   HstsIncludeSubDomains     { get; private set; }
            public bool   HstsPreload               { get; private set; }
            public bool   RemoveServerHeader        { get; private set; }

            // CSP padrão: compatível com Bootstrap 5 + jQuery 3 do projeto (carregados de /Scripts
            // e /Content). 'unsafe-inline' em script-src/style-src é necessário enquanto views
            // tiverem <script> / style="" inline — futura melhoria: migrar para nonces (ver SECURITY.md).
            //
            // data: em img-src cobre imagens base64 inline (usadas em alguns previews de upload).
            private const string DefaultCsp =
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline'; " +
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
                "img-src 'self' data: blob:; " +
                "font-src 'self' data: https://fonts.gstatic.com; " +
                "connect-src 'self'; " +
                "form-action 'self'; " +
                "frame-ancestors 'none'; " +
                "base-uri 'self'; " +
                "object-src 'none'; " +
                "upgrade-insecure-requests";

            private const string DefaultPermissionsPolicy =
                "geolocation=(), microphone=(), camera=(), payment=(), " +
                "usb=(), magnetometer=(), gyroscope=(), accelerometer=(), " +
                "fullscreen=(self)";

            public static SecurityOptions LoadFromAppSettings()
            {
                return new SecurityOptions
                {
                    ContentSecurityPolicy     = GetString("Security:ContentSecurityPolicy", DefaultCsp),
                    XFrameOptions             = GetString("Security:XFrameOptions",         "DENY"),
                    ReferrerPolicy            = GetString("Security:ReferrerPolicy",        "strict-origin-when-cross-origin"),
                    PermissionsPolicy         = GetString("Security:PermissionsPolicy",     DefaultPermissionsPolicy),
                    CrossOriginOpenerPolicy   = GetString("Security:CrossOriginOpenerPolicy",   "same-origin"),
                    CrossOriginResourcePolicy = GetString("Security:CrossOriginResourcePolicy", "same-origin"),
                    EnableHsts                = GetBool  ("Security:EnableHsts",                true),
                    HstsMaxAgeSeconds         = GetInt   ("Security:HstsMaxAgeSeconds",         31536000),
                    HstsIncludeSubDomains     = GetBool  ("Security:HstsIncludeSubDomains",     true),
                    HstsPreload               = GetBool  ("Security:HstsPreload",               false),
                    RemoveServerHeader        = GetBool  ("Security:RemoveServerHeader",        true)
                };
            }

            private static string GetString(string key, string fallback)
            {
                var v = ConfigurationManager.AppSettings[key];
                return string.IsNullOrWhiteSpace(v) ? fallback : v;
            }

            private static int GetInt(string key, int fallback)
            {
                var v = ConfigurationManager.AppSettings[key];
                int parsed;
                return int.TryParse(v, out parsed) ? parsed : fallback;
            }

            private static bool GetBool(string key, bool fallback)
            {
                var v = ConfigurationManager.AppSettings[key];
                bool parsed;
                return bool.TryParse(v, out parsed) ? parsed : fallback;
            }
        }
    }
}
