using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using ATLAS_ERP.Infrastructure;

namespace ATLAS_ERP.Controllers
{
    /// <summary>
    /// Proxy server-side para APIs externas de CEP e CNPJ.
    ///
    /// Motivação: o CSP do sistema define connect-src 'self', bloqueando fetch()
    /// direto do browser para brasilapi.com.br. Este controller recebe a requisição
    /// do browser (mesma origem, CSP ok), consulta a API externa via HttpClient
    /// e retorna o JSON ao browser sem expor a URL externa no client.
    ///
    /// Segurança:
    ///   - Requer sessão autenticada (nega 401 para anônimos)
    ///   - Valida formato antes de chamar a API externa (só dígitos, tamanho exato)
    ///   - Não repassa parâmetros livres — monta a URL com dados validados
    ///   - Timeout de 8 s para não bloquear a thread indefinidamente
    ///   - Erros logados via AppLogger (sem expor stack trace ao browser)
    /// </summary>
    public class ConsultaController : Controller
    {
        // HttpClient estático — evita socket exhaustion; DNS é resolvido uma vez.
        // Em .NET 4.7.2 com IIS é seguro compartilhar entre requests concorrentes.
        private static readonly HttpClient _http = CriarHttpClient();

        private static HttpClient CriarHttpClient()
        {
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
            client.DefaultRequestHeaders.Add("User-Agent", "ATLAS-ERP-Proxy/1.0");
            return client;
        }

        // ── CEP ────────────────────────────────────────────────────────────────
        // GET /Consulta/Cep?cep=01310100
        // Proxy para https://brasilapi.com.br/api/cep/v2/{cep}
        [HttpGet]
        public async Task<ActionResult> Cep(string cep)
        {
            if (Session[SessionKeys.UsuarioLogado] == null)
                return ErroJson(401, "Não autenticado");

            var digits = Regex.Replace(cep ?? "", @"\D", "");
            if (digits.Length != 8)
                return ErroJson(400, "CEP deve ter 8 dígitos");

            try
            {
                var resp = await _http.GetAsync("https://brasilapi.com.br/api/cep/v2/" + digits);
                if (!resp.IsSuccessStatusCode)
                    return ErroJson(404, "CEP não encontrado");

                var json = await resp.Content.ReadAsStringAsync();
                return Content(json, "application/json");
            }
            catch (TaskCanceledException)
            {
                return ErroJson(504, "Timeout na consulta de CEP");
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "consulta_cep_falhou cep=" + digits);
                return ErroJson(502, "Erro ao consultar CEP");
            }
        }

        // ── CNPJ ───────────────────────────────────────────────────────────────
        // GET /Consulta/Cnpj?cnpj=12345678000199
        // Proxy para https://brasilapi.com.br/api/cnpj/v1/{cnpj}
        [HttpGet]
        public async Task<ActionResult> Cnpj(string cnpj)
        {
            if (Session[SessionKeys.UsuarioLogado] == null)
                return ErroJson(401, "Não autenticado");

            var digits = Regex.Replace(cnpj ?? "", @"\D", "");
            if (digits.Length != 14)
                return ErroJson(400, "CNPJ deve ter 14 dígitos");

            try
            {
                var resp = await _http.GetAsync("https://brasilapi.com.br/api/cnpj/v1/" + digits);
                if (!resp.IsSuccessStatusCode)
                    return ErroJson(404, "CNPJ não encontrado");

                var json = await resp.Content.ReadAsStringAsync();
                return Content(json, "application/json");
            }
            catch (TaskCanceledException)
            {
                return ErroJson(504, "Timeout na consulta de CNPJ");
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "consulta_cnpj_falhou cnpj=" + digits);
                return ErroJson(502, "Erro ao consultar CNPJ");
            }
        }

        // Retorna JSON de erro com status HTTP correto.
        // O browser verifica r.ok (true para 2xx), então status != 200 dispara o .catch().
        private ActionResult ErroJson(int status, string mensagem)
        {
            Response.StatusCode = status;
            return Json(new { erro = mensagem }, JsonRequestBehavior.AllowGet);
        }
    }
}
