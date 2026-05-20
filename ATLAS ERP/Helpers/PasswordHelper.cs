using System;
using System.Security.Cryptography;
using System.Text;

namespace ATLAS_ERP.Helpers
{
    /// <summary>
    /// Hash de senha com PBKDF2-SHA256 + salt aleatório de 16 bytes + 100k iterações.
    /// Formato armazenado: "pbkdf2-sha256$&lt;iters&gt;$&lt;salt-base64&gt;$&lt;hash-base64&gt;".
    ///
    /// Compatibilidade retroativa: hashes legados em SHA256 puro (Base64) continuam validados
    /// pelo método Verificar(). Após login bem-sucedido com hash legado, PrecisaRehash() retorna
    /// true e o caller pode regravar a senha já no formato novo (rehash transparente).
    /// </summary>
    public static class PasswordHelper
    {
        private const int    SaltSize   = 16;
        private const int    HashSize   = 32;
        private const int    Iterations = 100_000;
        private const string Algorithm  = "pbkdf2-sha256";
        private const string FormatPrefix = "pbkdf2-sha256$";

        /// <summary>
        /// Gera hash novo no formato PBKDF2.
        /// Sempre usado para escrever no banco (cadastro, alteração, rehash).
        /// </summary>
        public static string HashSenha(string senha)
        {
            if (senha == null) senha = "";
            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);
            var hash = Pbkdf2(senha, salt, Iterations, HashSize);
            return Algorithm + "$" + Iterations.ToString(System.Globalization.CultureInfo.InvariantCulture)
                 + "$" + Convert.ToBase64String(salt)
                 + "$" + Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Compara senha em texto plano com hash armazenado.
        /// Aceita formato novo (PBKDF2) e legado (SHA256 base64 puro).
        /// </summary>
        public static bool Verificar(string senha, string hashArmazenado)
        {
            if (string.IsNullOrEmpty(hashArmazenado) || senha == null) return false;

            if (hashArmazenado.StartsWith(FormatPrefix, StringComparison.Ordinal))
                return VerificarPbkdf2(senha, hashArmazenado);

            return VerificarSha256Legado(senha, hashArmazenado);
        }

        /// <summary>
        /// True quando a senha está em formato legado e deve ser regravada com HashSenha
        /// pelo caller após login bem-sucedido. Implementa upgrade transparente.
        /// </summary>
        public static bool PrecisaRehash(string hashArmazenado)
        {
            if (string.IsNullOrEmpty(hashArmazenado)) return false;
            return !hashArmazenado.StartsWith(FormatPrefix, StringComparison.Ordinal);
        }

        private static bool VerificarPbkdf2(string senha, string hashArmazenado)
        {
            try
            {
                var partes = hashArmazenado.Split('$');
                if (partes.Length != 4) return false;
                if (!int.TryParse(partes[1], System.Globalization.NumberStyles.Integer,
                                  System.Globalization.CultureInfo.InvariantCulture, out int iter))
                    return false;
                var salt = Convert.FromBase64String(partes[2]);
                var hash = Convert.FromBase64String(partes[3]);
                var computed = Pbkdf2(senha, salt, iter, hash.Length);
                return ConstantTimeEquals(computed, hash);
            }
            catch
            {
                return false;
            }
        }

        private static bool VerificarSha256Legado(string senha, string hashArmazenado)
        {
            using (var sha256 = SHA256.Create())
            {
                var computed = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
                var computedB64 = Convert.ToBase64String(computed);
                return string.Equals(computedB64, hashArmazenado, StringComparison.Ordinal);
            }
        }

        private static byte[] Pbkdf2(string senha, byte[] salt, int iterations, int hashSize)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(senha, salt, iterations, HashAlgorithmName.SHA256))
                return pbkdf2.GetBytes(hashSize);
        }

        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
