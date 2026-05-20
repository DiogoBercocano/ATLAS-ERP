using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;

namespace ATLAS_ERP.Infrastructure
{
    /// <summary>
    /// Logger estruturado com saída em JSON Lines, correlation IDs (UsuarioId, EmpresaId,
    /// RequestId) extraídos do HttpContext atual e arquivo rotativo por dia em ~/App_Data/logs/.
    /// Sem dependência externa — substitui Trace.TraceError por algo auditável.
    ///
    /// Migração futura para Serilog: substituir o body de WriteLog() por chamada equivalente
    /// (Log.Write(level, ex, template, props)). API pública compatível com Serilog.
    /// </summary>
    public static class AppLogger
    {
        private static readonly object _writeLock = new object();
        private static string _logDirectory;

        public static void Info(string messageTemplate, params object[] properties)
            => WriteLog("INFO", null, messageTemplate, properties);

        public static void Warn(string messageTemplate, params object[] properties)
            => WriteLog("WARN", null, messageTemplate, properties);

        public static void Error(string messageTemplate, params object[] properties)
            => WriteLog("ERROR", null, messageTemplate, properties);

        public static void Error(Exception exception, string messageTemplate, params object[] properties)
            => WriteLog("ERROR", exception, messageTemplate, properties);

        /// <summary>
        /// Eventos de auditoria (login, criação/edição/exclusão de entidades sensíveis).
        /// Sempre escreve, mesmo em produção. Use AUDIT como nível para filtros posteriores.
        /// </summary>
        public static void Audit(string action, params object[] properties)
            => WriteLog("AUDIT", null, action, properties);

        private static void WriteLog(string level, Exception ex, string template, object[] properties)
        {
            try
            {
                var entry = new Dictionary<string, object>
                {
                    ["timestamp"] = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                    ["level"]     = level,
                    ["message"]   = FormatMessage(template, properties),
                    ["thread"]    = Thread.CurrentThread.ManagedThreadId
                };

                EnrichComContexto(entry);

                if (ex != null)
                {
                    entry["exception"] = new Dictionary<string, object>
                    {
                        ["type"]    = ex.GetType().FullName,
                        ["message"] = ex.Message,
                        ["stack"]   = ex.StackTrace
                    };
                }

                if (properties != null && properties.Length > 0)
                {
                    var props = new Dictionary<string, object>();
                    for (int i = 0; i < properties.Length; i++)
                        props["arg" + i] = properties[i]?.ToString();
                    entry["properties"] = props;
                }

                var json = SerializarJson(entry);

                lock (_writeLock)
                {
                    var path = GetLogFilePath();
                    File.AppendAllText(path, json + Environment.NewLine, Encoding.UTF8);
                }

                Trace.WriteLine("[" + level + "] " + entry["message"]);
            }
            catch
            {
                // logger nunca lança — falha silenciosa preserva fluxo principal
            }
        }

        private static void EnrichComContexto(Dictionary<string, object> entry)
        {
            try
            {
                var ctx = HttpContext.Current;
                if (ctx == null) return;

                if (ctx.Items != null && !ctx.Items.Contains("RequestId"))
                    ctx.Items["RequestId"] = Guid.NewGuid().ToString("N").Substring(0, 12);
                entry["requestId"] = ctx.Items?["RequestId"];

                var session = ctx.Session;
                if (session != null)
                {
                    if (session["UsuarioId"] != null) entry["usuarioId"] = session["UsuarioId"];
                    if (session["EmpresaId"] != null) entry["empresaId"] = session["EmpresaId"];
                    if (session["CargoNome"] != null) entry["cargo"]     = session["CargoNome"];
                }

                if (ctx.Request != null)
                {
                    entry["path"]   = ctx.Request.Path;
                    entry["method"] = ctx.Request.HttpMethod;
                }
            }
            catch
            {
                // sem HttpContext (ex: thread de background) — segue sem enrichment
            }
        }

        private static string FormatMessage(string template, object[] properties)
        {
            if (string.IsNullOrEmpty(template)) return "";
            if (properties == null || properties.Length == 0) return template;
            try
            {
                return string.Format(CultureInfo.InvariantCulture, template, properties);
            }
            catch
            {
                return template;
            }
        }

        private static string GetLogFilePath()
        {
            if (_logDirectory == null)
            {
                _logDirectory = HttpContext.Current != null
                    ? HttpContext.Current.Server.MapPath("~/App_Data/logs/")
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "logs");
                if (!Directory.Exists(_logDirectory))
                    Directory.CreateDirectory(_logDirectory);
            }
            return Path.Combine(_logDirectory, "atlas-" + DateTime.UtcNow.ToString("yyyy-MM-dd") + ".log");
        }

        private static string SerializarJson(Dictionary<string, object> entry)
        {
            var sb = new StringBuilder("{");
            bool first = true;
            foreach (var kv in entry)
            {
                if (!first) sb.Append(",");
                first = false;
                sb.Append('"').Append(EscapeJsonString(kv.Key)).Append("\":");
                AppendValor(sb, kv.Value);
            }
            sb.Append('}');
            return sb.ToString();
        }

        private static void AppendValor(StringBuilder sb, object value)
        {
            if (value == null) { sb.Append("null"); return; }
            if (value is Dictionary<string, object> dict)
            {
                sb.Append('{');
                bool first = true;
                foreach (var kv in dict)
                {
                    if (!first) sb.Append(",");
                    first = false;
                    sb.Append('"').Append(EscapeJsonString(kv.Key)).Append("\":");
                    AppendValor(sb, kv.Value);
                }
                sb.Append('}');
                return;
            }
            if (value is bool b) { sb.Append(b ? "true" : "false"); return; }
            if (value is int || value is long || value is double || value is decimal)
            {
                sb.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
                return;
            }
            sb.Append('"').Append(EscapeJsonString(value.ToString())).Append('"');
        }

        private static string EscapeJsonString(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder(s.Length + 4);
            for (int i = 0; i < s.Length; i++)
            {
                var c = s[i];
                switch (c)
                {
                    case '"':  sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20)
                            sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else
                            sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
