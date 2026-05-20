using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ATLAS_ERP.Infrastructure
{
    /// <summary>
    /// Validação centralizada de uploads — single source of truth para imagens
    /// (foto de produto, logo de empresa, qualquer adição futura).
    ///
    /// Defesa em camadas:
    ///   1. Extensão na whitelist
    ///   2. ContentType (MIME enviado pelo browser) coerente — defesa fraca, complementa
    ///   3. Tamanho máximo configurável
    ///   4. Magic bytes (assinatura binária dos primeiros bytes do arquivo) — decisivo
    ///   5. Nome de arquivo final gerado server-side (GUID + extensão segura)
    ///      — elimina path traversal, RCE via dupla extensão, sobrescrita de arquivos
    ///
    /// Formatos aceitos: JPEG, PNG, GIF, WebP. SVG é REJEITADO (vetor para XSS via &lt;script&gt;).
    /// </summary>
    public static class UploadValidator
    {
        // Limite default — também enforced em <httpRuntime maxRequestLength> e
        // <requestLimits maxAllowedContentLength> no Web.config (10MB).
        // Override por chamada permite limites menores por contexto (ex: logo = 2MB).
        public const int DefaultMaxBytes = 5 * 1024 * 1024;     // 5 MB
        public const int LogoMaxBytes    = 2 * 1024 * 1024;     // 2 MB
        public const int MagicBytePeek   = 12;                  // suficiente para todos os formatos suportados

        private static readonly HashSet<string> ExtensoesPermitidas =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private static readonly HashSet<string> MimesPermitidos =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "image/jpeg", "image/pjpeg", "image/png", "image/gif", "image/webp" };

        // Extensões perigosas ou de imagem que NÃO devem aparecer como penúltima extensão.
        // Detecta "shell.aspx.jpg" e "foto.png.jpg", mas permite "foto 20.50.30.jpeg".
        private static readonly HashSet<string> ExtensoesBloqueadasNaBase =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".aspx", ".ashx", ".asmx", ".ascx", ".cshtml", ".vbhtml",
                ".php", ".php3", ".php4", ".php5", ".phtml",
                ".exe", ".dll", ".bat", ".cmd", ".ps1", ".sh",
                ".config", ".cs", ".vb", ".resx", ".xml", ".json",
                ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".bmp", ".tiff",
            };

        /// <summary>
        /// Valida um upload de imagem. Não escreve em disco — apenas inspeciona.
        /// Quem chama decide o que fazer com o resultado (gravar, recusar, logar).
        /// </summary>
        public static UploadResult Validar(HttpPostedFileBase arquivo, string contexto, int? maxBytes = null)
        {
            var limite = maxBytes ?? DefaultMaxBytes;

            if (arquivo == null || arquivo.ContentLength == 0)
                return UploadResult.ParaVazio(contexto);

            if (arquivo.ContentLength > limite)
                return UploadResult.ParaRejeitado(contexto, arquivo.FileName,
                    $"tamanho_excedido bytes={arquivo.ContentLength} limite={limite}");

            // FileName pode vir vazio em alguns clientes ou conter caminho completo
            // (browsers antigos / IE). Normaliza para apenas o nome.
            var nomeOriginal = SafeFileName(arquivo.FileName);
            if (string.IsNullOrWhiteSpace(nomeOriginal))
                return UploadResult.ParaRejeitado(contexto, arquivo.FileName, "nome_invalido");

            var extensao = Path.GetExtension(nomeOriginal).ToLowerInvariant();
            if (!ExtensoesPermitidas.Contains(extensao))
                return UploadResult.ParaRejeitado(contexto, nomeOriginal,
                    $"extensao_nao_permitida ext={extensao}");

            // Detectar dupla extensão perigosa (ex: "shell.aspx.jpg").
            // Conta pontos gerais era falso-positivo para fotos de celular ("20.50.30.jpeg").
            // Agora verifica se o nome SEM a última extensão ainda termina com extensão bloqueada.
            var extensaoBase = Path.GetExtension(
                Path.GetFileNameWithoutExtension(nomeOriginal)).ToLowerInvariant();
            if (!string.IsNullOrEmpty(extensaoBase) && ExtensoesBloqueadasNaBase.Contains(extensaoBase))
                return UploadResult.ParaRejeitado(contexto, nomeOriginal, "dupla_extensao");

            // MIME do browser — defesa fraca (cliente pode mentir), mas detecta tentativas
            // ingênuas. Falha aqui significa que extensão e ContentType discordam.
            if (!string.IsNullOrEmpty(arquivo.ContentType) &&
                !MimesPermitidos.Contains(arquivo.ContentType))
                return UploadResult.ParaRejeitado(contexto, nomeOriginal,
                    $"mime_invalido ct={arquivo.ContentType}");

            // Magic bytes — defesa decisiva. Lê os primeiros bytes do stream e confere
            // contra assinaturas conhecidas. Independe de extensão ou MIME do cliente.
            var assinatura = InspecionarAssinatura(arquivo.InputStream);
            if (assinatura == FormatoImagem.Desconhecido)
                return UploadResult.ParaAssinaturaInvalida(contexto, nomeOriginal,
                    $"magic_bytes_nao_correspondem ext={extensao}");

            if (!AssinaturaCompativelComExtensao(assinatura, extensao))
                return UploadResult.ParaAssinaturaInvalida(contexto, nomeOriginal,
                    $"assinatura_incompativel ext={extensao} detectado={assinatura}");

            // Sucesso: gera nome final neutro (GUID + extensão).
            // NUNCA usa o nome original do cliente — elimina path traversal e nome ofensivo.
            var nomeFinal = Guid.NewGuid().ToString("N") + extensao;
            return UploadResult.ParaSucesso(contexto, nomeOriginal, nomeFinal, assinatura);
        }

        /// <summary>
        /// Salva o arquivo em disco APÓS validação bem-sucedida. Cria o diretório se
        /// não existir. Retorna o caminho relativo público para uso em &lt;img src&gt;.
        /// </summary>
        public static string Persistir(HttpPostedFileBase arquivo, UploadResult resultado, string pastaFisica, string pastaRelativa)
        {
            if (!resultado.Sucesso)
                throw new InvalidOperationException("Persistir chamado com resultado inválido");

            if (!Directory.Exists(pastaFisica))
                Directory.CreateDirectory(pastaFisica);

            // Reset do stream antes de salvar — InspecionarAssinatura consumiu os primeiros bytes
            if (arquivo.InputStream.CanSeek)
                arquivo.InputStream.Position = 0;

            var destino = Path.Combine(pastaFisica, resultado.NomeFinal);

            // Caminho absoluto resolvido — garantia adicional contra path traversal mesmo que
            // pastaRelativa contenha "../" (não deveria, mas é defesa em profundidade).
            var destinoCanonico = Path.GetFullPath(destino);
            var pastaCanonica   = Path.GetFullPath(pastaFisica);
            if (!destinoCanonico.StartsWith(pastaCanonica, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("path_traversal_detectado");

            arquivo.SaveAs(destinoCanonico);

            var sep = pastaRelativa.EndsWith("/") ? string.Empty : "/";
            return pastaRelativa + sep + resultado.NomeFinal;
        }

        private static FormatoImagem InspecionarAssinatura(Stream stream)
        {
            if (stream == null || !stream.CanRead) return FormatoImagem.Desconhecido;

            var posicaoOriginal = stream.CanSeek ? stream.Position : 0;
            try
            {
                if (stream.CanSeek) stream.Position = 0;

                var buffer = new byte[MagicBytePeek];
                var lidos  = stream.Read(buffer, 0, buffer.Length);
                if (lidos < 4) return FormatoImagem.Desconhecido;

                // JPEG: FF D8 FF
                if (lidos >= 3 && buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF)
                    return FormatoImagem.Jpeg;

                // PNG: 89 50 4E 47 0D 0A 1A 0A
                if (lidos >= 8 &&
                    buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47 &&
                    buffer[4] == 0x0D && buffer[5] == 0x0A && buffer[6] == 0x1A && buffer[7] == 0x0A)
                    return FormatoImagem.Png;

                // GIF: "GIF87a" ou "GIF89a"
                if (lidos >= 6 &&
                    buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 &&
                    buffer[3] == 0x38 && (buffer[4] == 0x37 || buffer[4] == 0x39) && buffer[5] == 0x61)
                    return FormatoImagem.Gif;

                // WebP: "RIFF" .... "WEBP"
                if (lidos >= 12 &&
                    buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46 &&
                    buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50)
                    return FormatoImagem.Webp;

                return FormatoImagem.Desconhecido;
            }
            finally
            {
                if (stream.CanSeek) stream.Position = posicaoOriginal;
            }
        }

        private static bool AssinaturaCompativelComExtensao(FormatoImagem formato, string ext)
        {
            switch (formato)
            {
                case FormatoImagem.Jpeg: return ext == ".jpg" || ext == ".jpeg";
                case FormatoImagem.Png:  return ext == ".png";
                case FormatoImagem.Gif:  return ext == ".gif";
                case FormatoImagem.Webp: return ext == ".webp";
                default:                 return false;
            }
        }

        private static string SafeFileName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            // Remove caminhos (IE6 mandava "C:\Users\..\foto.jpg" como FileName)
            return Path.GetFileName(raw.Trim());
        }
    }

    public enum FormatoImagem { Desconhecido, Jpeg, Png, Gif, Webp }

    public enum UploadStatus { Vazio, Sucesso, Rejeitado, AssinaturaInvalida }

    /// <summary>
    /// Resultado tipado da validação. Imutável. Contém tudo que o caller precisa
    /// para decidir o próximo passo e emitir audit log sem informação sensível.
    /// </summary>
    public sealed class UploadResult
    {
        public UploadStatus   Status        { get; private set; }
        public bool           Sucesso       => Status == UploadStatus.Sucesso;
        public bool           Vazio         => Status == UploadStatus.Vazio;
        public string         Contexto      { get; private set; }
        public string         NomeOriginal  { get; private set; }
        public string         NomeFinal     { get; private set; }
        public FormatoImagem  Formato       { get; private set; }
        public string         Motivo        { get; private set; }

        private UploadResult() { }

        internal static UploadResult ParaVazio(string contexto) =>
            new UploadResult { Status = UploadStatus.Vazio, Contexto = contexto };

        internal static UploadResult ParaSucesso(string contexto, string original, string final, FormatoImagem formato) =>
            new UploadResult { Status = UploadStatus.Sucesso, Contexto = contexto,
                               NomeOriginal = original, NomeFinal = final, Formato = formato };

        internal static UploadResult ParaRejeitado(string contexto, string original, string motivo) =>
            new UploadResult { Status = UploadStatus.Rejeitado, Contexto = contexto,
                               NomeOriginal = original, Motivo = motivo };

        internal static UploadResult ParaAssinaturaInvalida(string contexto, string original, string motivo) =>
            new UploadResult { Status = UploadStatus.AssinaturaInvalida, Contexto = contexto,
                               NomeOriginal = original, Motivo = motivo };

        /// <summary>
        /// Emite o evento de audit log apropriado via AppLogger. Centraliza os
        /// 3 eventos canônicos: upload_success, upload_rejected, upload_invalid_signature.
        /// Nunca expõe stack trace ou caminhos absolutos — só metadata segura.
        /// </summary>
        public void Auditar(int? usuarioId, int? empresaId)
        {
            switch (Status)
            {
                case UploadStatus.Sucesso:
                    AppLogger.Audit("upload_success contexto={0} usuarioId={1} empresaId={2} formato={3} nomeFinal={4}",
                                    Contexto, usuarioId, empresaId, Formato, NomeFinal);
                    break;

                case UploadStatus.Rejeitado:
                    AppLogger.Audit("upload_rejected contexto={0} usuarioId={1} empresaId={2} motivo={3} nome={4}",
                                    Contexto, usuarioId, empresaId, Motivo, NomeOriginal);
                    break;

                case UploadStatus.AssinaturaInvalida:
                    AppLogger.Audit("upload_invalid_signature contexto={0} usuarioId={1} empresaId={2} motivo={3} nome={4}",
                                    Contexto, usuarioId, empresaId, Motivo, NomeOriginal);
                    break;

                case UploadStatus.Vazio:
                    // Não loga — campo vazio em formulário não é evento de segurança
                    break;
            }
        }
    }
}
