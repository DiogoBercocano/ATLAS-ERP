using System;
using System.Security.Cryptography;
using System.Text;

namespace ATLAS_ERP.Helpers
{
    /// <summary>
    /// Utilitário para hash seguro de senhas com salt individual (SHA-256 + salt).
    /// Formato armazenado: "base64(salt):base64(hash)"
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// Gera um hash seguro da senha com salt aleatório.
        /// </summary>
        public static string Hash(string senha)
        {
            var saltBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(saltBytes);

            var salt = Convert.ToBase64String(saltBytes);
            var hash = ComputeHash(senha, salt);
            return salt + ":" + hash;
        }

        /// <summary>
        /// Verifica se a senha informada corresponde ao hash armazenado.
        /// </summary>
        public static bool Verify(string senha, string hashArmazenado)
        {
            if (string.IsNullOrEmpty(senha) || string.IsNullOrEmpty(hashArmazenado))
                return false;

            var partes = hashArmazenado.Split(new[] { ':' }, 2);
            if (partes.Length != 2)
                return false;

            var salt = partes[0];
            var hashEsperado = partes[1];
            var hashCalculado = ComputeHash(senha, salt);

            return SlowEquals(hashCalculado, hashEsperado);
        }

        private static string ComputeHash(string senha, string salt)
        {
            var combined = Encoding.UTF8.GetBytes(salt + ":" + senha);
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(combined);
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Comparação em tempo constante para evitar timing attacks.
        /// </summary>
        private static bool SlowEquals(string a, string b)
        {
            if (a == null || b == null) return false;
            var diff = (uint)a.Length ^ (uint)b.Length;
            var len = Math.Min(a.Length, b.Length);
            for (int i = 0; i < len; i++)
                diff |= (uint)(a[i] ^ b[i]);
            return diff == 0;
        }
    }
}
