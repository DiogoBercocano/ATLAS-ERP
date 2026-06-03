using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using ATLAS_ERP.Infrastructure;
using Newtonsoft.Json.Linq;

namespace ATLAS_ERP.Controllers
{
    public class ConsultaController : Controller
    {
        // TLS 1.2 é garantido via ServicePointManager em Global.asax.
        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        static ConsultaController()
        {
            _http.DefaultRequestHeaders.Add("User-Agent", "ATLAS-ERP/1.0");
        }

        // GET /Consulta/Cep?cep=01310100  →  proxy para https://viacep.com.br/ws/{cep}/json//
        // Endpoint público: ViaCEP é API pública sem autenticação; também usada na página de cadastro.
        [HttpGet]
        public async Task<ActionResult> Cep(string cep)
        {
            var digits = Regex.Replace(cep ?? "", @"\D", "");
            if (digits.Length != 8)
                return ErroJson(400, "CEP deve ter 8 dígitos");

            try
            {
                var resp = await _http.GetAsync("https://viacep.com.br/ws/" + digits + "/json/");
                if (!resp.IsSuccessStatusCode)
                    return ErroJson(404, "CEP não encontrado");

                var json = await resp.Content.ReadAsStringAsync();

                // ViaCEP retorna {"erro": "true"} com HTTP 200 quando o CEP não existe
                var obj = JObject.Parse(json);
                if (obj["erro"] != null)
                    return ErroJson(404, "CEP não encontrado");

                return Content(json, "application/json");
            }
            catch (TaskCanceledException)
            {
                return ErroJson(504, "Timeout na consulta de CEP");
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "consulta_cep_falhou cep=" + digits);
                return ErroJson(502, "Erro ao consultar CEP: " + ex.GetType().Name);
            }
        }

        // GET /Consulta/Cnpj?cnpj=12345678000199
        // Tenta BrasilAPI; fallback para ReceitaWS normalizando os campos para o mesmo formato.
        // Endpoint público: dados da Receita Federal são públicos; usada também no cadastro de empresa.
        [HttpGet]
        public async Task<ActionResult> Cnpj(string cnpj)
        {
            var digits = Regex.Replace(cnpj ?? "", @"\D", "");
            if (digits.Length != 14)
                return ErroJson(400, "CNPJ deve ter 14 dígitos");

            // Tentativa 1: BrasilAPI
            try
            {
                var r1 = await _http.GetAsync("https://brasilapi.com.br/api/cnpj/v1/" + digits);
                if (r1.IsSuccessStatusCode)
                    return Content(await r1.Content.ReadAsStringAsync(), "application/json");
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "brasilapi_cnpj_falhou cnpj=" + digits);
            }

            // Tentativa 2: ReceitaWS — normaliza para o formato BrasilAPI que as views esperam
            try
            {
                var r2 = await _http.GetAsync("https://receitaws.com.br/v1/cnpj/" + digits);
                if (!r2.IsSuccessStatusCode)
                    return ErroJson(404, "CNPJ não encontrado (status " + (int)r2.StatusCode + ")");

                var body = await r2.Content.ReadAsStringAsync();
                var obj  = JObject.Parse(body);

                if (obj["status"]?.ToString() == "ERROR")
                    return ErroJson(404, obj["message"]?.ToString() ?? "CNPJ não encontrado");

                var normalizado = new
                {
                    razao_social   = obj["nome"]?.ToString()     ?? "",
                    nome_fantasia  = obj["fantasia"]?.ToString() ?? "",
                    email          = obj["email"]?.ToString()    ?? "",
                    ddd_telefone_1 = Regex.Replace(obj["telefone"]?.ToString() ?? "", @"\D", ""),
                    cep            = Regex.Replace(obj["cep"]?.ToString()       ?? "", @"\D", ""),
                    logradouro     = obj["logradouro"]?.ToString() ?? "",
                    numero         = obj["numero"]?.ToString()     ?? "",
                    bairro         = obj["bairro"]?.ToString()     ?? "",
                    municipio      = obj["municipio"]?.ToString()  ?? "",
                    uf             = obj["uf"]?.ToString()         ?? ""
                };

                return Json(normalizado, JsonRequestBehavior.AllowGet);
            }
            catch (TaskCanceledException)
            {
                return ErroJson(504, "Timeout na consulta de CNPJ");
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "receitaws_cnpj_falhou cnpj=" + digits);
                return ErroJson(502, "Erro ao consultar CNPJ: " + ex.GetType().Name);
            }
        }

        private ActionResult ErroJson(int status, string mensagem)
        {
            Response.StatusCode = status;
            return Json(new { erro = mensagem }, JsonRequestBehavior.AllowGet);
        }
    }
}
