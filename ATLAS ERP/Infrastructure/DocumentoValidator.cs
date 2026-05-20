using System.Linq;
using System.Text;

namespace ATLAS_ERP.Infrastructure
{
    /// <summary>
    /// Validador e normalizador de CPF/CNPJ (algoritmo de dígitos verificadores).
    /// Persistir SEMPRE normalizado (apenas dígitos). Formatar apenas no display.
    /// </summary>
    public static class DocumentoValidator
    {
        public static string Normalizar(string documento)
        {
            if (string.IsNullOrEmpty(documento)) return string.Empty;
            var sb = new StringBuilder(documento.Length);
            for (int i = 0; i < documento.Length; i++)
            {
                var c = documento[i];
                if (c >= '0' && c <= '9') sb.Append(c);
            }
            return sb.ToString();
        }

        public static bool EhCpf(string documento)
            => Normalizar(documento).Length == 11;

        public static bool EhCnpj(string documento)
            => Normalizar(documento).Length == 14;

        public static bool ValidarCPF(string cpf)
        {
            var digits = Normalizar(cpf);
            if (digits.Length != 11) return false;
            if (digits.All(c => c == digits[0])) return false;

            int s1 = 0;
            for (int i = 0; i < 9; i++) s1 += (digits[i] - '0') * (10 - i);
            int r1 = (s1 * 10) % 11;
            if (r1 == 10 || r1 == 11) r1 = 0;
            if (r1 != (digits[9] - '0')) return false;

            int s2 = 0;
            for (int i = 0; i < 10; i++) s2 += (digits[i] - '0') * (11 - i);
            int r2 = (s2 * 10) % 11;
            if (r2 == 10 || r2 == 11) r2 = 0;
            return r2 == (digits[10] - '0');
        }

        public static bool ValidarCNPJ(string cnpj)
        {
            var digits = Normalizar(cnpj);
            if (digits.Length != 14) return false;
            if (digits.All(c => c == digits[0])) return false;

            int[] pesos1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] pesos2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            int s1 = 0;
            for (int i = 0; i < 12; i++) s1 += (digits[i] - '0') * pesos1[i];
            int r1 = s1 % 11;
            int dv1 = r1 < 2 ? 0 : 11 - r1;
            if (dv1 != (digits[12] - '0')) return false;

            int s2 = 0;
            for (int i = 0; i < 13; i++) s2 += (digits[i] - '0') * pesos2[i];
            int r2 = s2 % 11;
            int dv2 = r2 < 2 ? 0 : 11 - r2;
            return dv2 == (digits[13] - '0');
        }

        /// <summary>
        /// Aceita CPF (11 dígitos) ou CNPJ (14 dígitos). False para qualquer outro tamanho.
        /// </summary>
        public static bool Validar(string documento)
        {
            var digits = Normalizar(documento);
            if (digits.Length == 11) return ValidarCPF(digits);
            if (digits.Length == 14) return ValidarCNPJ(digits);
            return false;
        }

        public static string FormatarCPF(string cpf)
        {
            var d = Normalizar(cpf);
            if (d.Length != 11) return cpf;
            return d.Substring(0, 3) + "." + d.Substring(3, 3) + "." + d.Substring(6, 3) + "-" + d.Substring(9, 2);
        }

        public static string FormatarCNPJ(string cnpj)
        {
            var d = Normalizar(cnpj);
            if (d.Length != 14) return cnpj;
            return d.Substring(0, 2) + "." + d.Substring(2, 3) + "." + d.Substring(5, 3) + "/" + d.Substring(8, 4) + "-" + d.Substring(12, 2);
        }

        /// <summary>
        /// Formata automaticamente (CPF se 11 dígitos, CNPJ se 14).
        /// </summary>
        public static string Formatar(string documento)
        {
            var d = Normalizar(documento);
            if (d.Length == 11) return FormatarCPF(d);
            if (d.Length == 14) return FormatarCNPJ(d);
            return documento;
        }
    }
}
