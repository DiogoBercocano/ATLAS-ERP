using System;
using System.Security.Cryptography;
using System.Text;
using ATLAS_ERP.Helpers;

namespace ATLAS_ERP.Tests
{
    [TestClass]
    public class PasswordHelperTests
    {
        // ── HashSenha ────────────────────────────────────────────────────────────

        [TestMethod]
        public void HashSenha_RetornaFormatoPbkdf2()
        {
            var hash = PasswordHelper.HashSenha("senha123");

            Assert.IsTrue(hash.StartsWith("pbkdf2-sha256$", StringComparison.Ordinal));
            Assert.AreEqual(4, hash.Split('$').Length);
        }

        [TestMethod]
        public void HashSenha_MesmaSenha_ProduzeHashesDiferentes()
        {
            // Cada chamada gera salt aleatório — hashes devem ser distintos
            var h1 = PasswordHelper.HashSenha("igual");
            var h2 = PasswordHelper.HashSenha("igual");

            Assert.AreNotEqual(h1, h2);
        }

        [TestMethod]
        public void HashSenha_SenhaVazia_NaoLanca()
        {
            // Não deve lançar exceção
            var hash = PasswordHelper.HashSenha("");
            Assert.IsNotNull(hash);
        }

        [TestMethod]
        public void HashSenha_SenhaNula_NaoLanca()
        {
            var hash = PasswordHelper.HashSenha(null!);
            Assert.IsNotNull(hash);
        }

        // ── Verificar ────────────────────────────────────────────────────────────

        [TestMethod]
        public void Verificar_SenhaCorreta_ReturnaTrue()
        {
            var hash = PasswordHelper.HashSenha("atlas@2026");
            Assert.IsTrue(PasswordHelper.Verificar("atlas@2026", hash));
        }

        [TestMethod]
        public void Verificar_SenhaErrada_ReturnaFalse()
        {
            var hash = PasswordHelper.HashSenha("correta");
            Assert.IsFalse(PasswordHelper.Verificar("errada", hash));
        }

        [TestMethod]
        public void Verificar_SenhaVazia_ReturnaFalse()
        {
            var hash = PasswordHelper.HashSenha("naoVazia");
            Assert.IsFalse(PasswordHelper.Verificar("", hash));
        }

        [TestMethod]
        public void Verificar_HashNull_ReturnaFalse()
        {
            Assert.IsFalse(PasswordHelper.Verificar("senha", null!));
        }

        [TestMethod]
        public void Verificar_HashVazio_ReturnaFalse()
        {
            Assert.IsFalse(PasswordHelper.Verificar("senha", ""));
        }

        [TestMethod]
        public void Verificar_HashMalformado_ReturnaFalse()
        {
            Assert.IsFalse(PasswordHelper.Verificar("senha", "pbkdf2-sha256$apenas_duas_partes"));
        }

        [TestMethod]
        public void Verificar_HashMalformado_SaltBase64Invalido_ReturnaFalse()
        {
            Assert.IsFalse(PasswordHelper.Verificar("senha", "pbkdf2-sha256$100000$!!!base64invalido!!!$abc"));
        }

        [TestMethod]
        public void Verificar_LegadoSha256_Correto_ReturnaTrue()
        {
            // SHA256 de "test" em base64 — hash legado sem prefixo pbkdf2
            const string senha = "test";
            var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
            var legado = Convert.ToBase64String(hashBytes);

            Assert.IsTrue(PasswordHelper.Verificar(senha, legado));
        }

        [TestMethod]
        public void Verificar_LegadoSha256_Errado_ReturnaFalse()
        {
            const string outroHash = "n4bQgYhMfWWaL+qgxVrQFaO/TxsrC4Is0V1sFbDwCgg="; // SHA256("test")
            Assert.IsFalse(PasswordHelper.Verificar("errado", outroHash));
        }

        // ── PrecisaRehash ────────────────────────────────────────────────────────

        [TestMethod]
        public void PrecisaRehash_HashPbkdf2_ReturnaFalse()
        {
            var hash = PasswordHelper.HashSenha("qualquer");
            Assert.IsFalse(PasswordHelper.PrecisaRehash(hash));
        }

        [TestMethod]
        public void PrecisaRehash_HashLegado_ReturnaTrue()
        {
            const string legado = "n4bQgYhMfWWaL+qgxVrQFaO/TxsrC4Is0V1sFbDwCgg=";
            Assert.IsTrue(PasswordHelper.PrecisaRehash(legado));
        }

        [TestMethod]
        public void PrecisaRehash_HashNull_ReturnaFalse()
        {
            Assert.IsFalse(PasswordHelper.PrecisaRehash(null!));
        }

        [TestMethod]
        public void PrecisaRehash_HashVazio_ReturnaFalse()
        {
            Assert.IsFalse(PasswordHelper.PrecisaRehash(""));
        }

        // ── Round-trip ────────────────────────────────────────────────────────────

        [TestMethod]
        public void HashSenha_Verificar_RoundTrip_Sucesso()
        {
            const string senha = "Atlas@2026#ERP";
            var hash = PasswordHelper.HashSenha(senha);
            Assert.IsTrue(PasswordHelper.Verificar(senha, hash));
            Assert.IsFalse(PasswordHelper.Verificar(senha + "x", hash));
        }

        [TestMethod]
        public void HashSenha_ComCaracteresEspeciais_VerificaCorretamente()
        {
            const string senha = "p@$$w0rd!<>&\"'";
            var hash = PasswordHelper.HashSenha(senha);
            Assert.IsTrue(PasswordHelper.Verificar(senha, hash));
        }
    }
}
