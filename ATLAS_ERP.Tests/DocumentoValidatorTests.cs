using ATLAS_ERP.Infrastructure;

namespace ATLAS_ERP.Tests
{
    [TestClass]
    public class DocumentoValidatorTests
    {
        // CPF e CNPJ válidos usados como constantes de teste
        private const string CpfValido      = "52998224725";   // dígitos normalizados
        private const string CpfFormatado   = "529.982.247-25";
        private const string CnpjValido     = "11222333000181"; // dígitos normalizados
        private const string CnpjFormatado  = "11.222.333/0001-81";

        // ── Normalizar ────────────────────────────────────────────────────────────

        [TestMethod]
        public void Normalizar_CpfFormatado_RetornaDigitos()
        {
            Assert.AreEqual("52998224725", DocumentoValidator.Normalizar(CpfFormatado));
        }

        [TestMethod]
        public void Normalizar_CnpjFormatado_RetornaDigitos()
        {
            Assert.AreEqual("11222333000181", DocumentoValidator.Normalizar(CnpjFormatado));
        }

        [TestMethod]
        public void Normalizar_JaNormalizado_RetornaMesmoValor()
        {
            Assert.AreEqual(CpfValido, DocumentoValidator.Normalizar(CpfValido));
        }

        [TestMethod]
        public void Normalizar_Null_RetornaVazio()
        {
            Assert.AreEqual(string.Empty, DocumentoValidator.Normalizar(null!));
        }

        [TestMethod]
        public void Normalizar_Vazio_RetornaVazio()
        {
            Assert.AreEqual(string.Empty, DocumentoValidator.Normalizar(""));
        }

        [TestMethod]
        public void Normalizar_ComEspacos_RemoveEspacos()
        {
            Assert.AreEqual("12345678901", DocumentoValidator.Normalizar("123 456 789 01"));
        }

        // ── EhCpf / EhCnpj ───────────────────────────────────────────────────────

        [TestMethod]
        public void EhCpf_11Digitos_ReturnaTrue()
        {
            Assert.IsTrue(DocumentoValidator.EhCpf(CpfValido));
        }

        [TestMethod]
        public void EhCpf_14Digitos_ReturnaFalse()
        {
            Assert.IsFalse(DocumentoValidator.EhCpf(CnpjValido));
        }

        [TestMethod]
        public void EhCnpj_14Digitos_ReturnaTrue()
        {
            Assert.IsTrue(DocumentoValidator.EhCnpj(CnpjValido));
        }

        [TestMethod]
        public void EhCnpj_11Digitos_ReturnaFalse()
        {
            Assert.IsFalse(DocumentoValidator.EhCnpj(CpfValido));
        }

        // ── ValidarCPF ────────────────────────────────────────────────────────────

        [TestMethod]
        public void ValidarCPF_ValorValido_ReturnaTrue()
        {
            Assert.IsTrue(DocumentoValidator.ValidarCPF(CpfValido));
        }

        [TestMethod]
        public void ValidarCPF_Formatado_ReturnaTrue()
        {
            Assert.IsTrue(DocumentoValidator.ValidarCPF(CpfFormatado));
        }

        [TestMethod]
        public void ValidarCPF_DigitosErrados_ReturnaFalse()
        {
            Assert.IsFalse(DocumentoValidator.ValidarCPF("52998224726")); // último dígito alterado
        }

        [TestMethod]
        public void ValidarCPF_TodosZeros_ReturnaFalse()
        {
            Assert.IsFalse(DocumentoValidator.ValidarCPF("00000000000"));
        }

        [TestMethod]
        public void ValidarCPF_TodosUns_ReturnaFalse()
        {
            Assert.IsFalse(DocumentoValidator.ValidarCPF("11111111111"));
        }

        [TestMethod]
        public void ValidarCPF_TodosNovos_ReturnaFalse()
        {
            Assert.IsFalse(DocumentoValidator.ValidarCPF("99999999999"));
        }

        [TestMethod]
        public void ValidarCPF_TamanhoErrado_ReturnaFalse()
        {
            Assert.IsFalse(DocumentoValidator.ValidarCPF("123456789")); // 9 dígitos
        }

        [TestMethod]
        public void ValidarCPF_Vazio_ReturnaFalse()
        {
            Assert.IsFalse(DocumentoValidator.ValidarCPF(""));
        }

        // ── ValidarCNPJ ───────────────────────────────────────────────────────────

        [TestMethod]
        public void ValidarCNPJ_ValorValido_ReturnaTrue()
        {
            Assert.IsTrue(DocumentoValidator.ValidarCNPJ(CnpjValido));
        }

        [TestMethod]
        public void ValidarCNPJ_Formatado_ReturnaTrue()
        {
            Assert.IsTrue(DocumentoValidator.ValidarCNPJ(CnpjFormatado));
        }

        [TestMethod]
        public void ValidarCNPJ_DigitosErrados_ReturnaFalse()
        {
            Assert.IsFalse(DocumentoValidator.ValidarCNPJ("11222333000182")); // último dígito alterado
        }

        [TestMethod]
        public void ValidarCNPJ_TodosZeros_ReturnaFalse()
        {
            Assert.IsFalse(DocumentoValidator.ValidarCNPJ("00000000000000"));
        }

        [TestMethod]
        public void ValidarCNPJ_TamanhoErrado_ReturnaFalse()
        {
            Assert.IsFalse(DocumentoValidator.ValidarCNPJ("1122233300018")); // 13 dígitos
        }

        [TestMethod]
        public void ValidarCNPJ_Vazio_ReturnaFalse()
        {
            Assert.IsFalse(DocumentoValidator.ValidarCNPJ(""));
        }

        // ── Validar (genérico CPF ou CNPJ) ───────────────────────────────────────

        [TestMethod]
        public void Validar_Cpf_ReturnaTrue()
        {
            Assert.IsTrue(DocumentoValidator.Validar(CpfValido));
        }

        [TestMethod]
        public void Validar_Cnpj_ReturnaTrue()
        {
            Assert.IsTrue(DocumentoValidator.Validar(CnpjValido));
        }

        [TestMethod]
        public void Validar_TamanhoIncompativel_ReturnaFalse()
        {
            Assert.IsFalse(DocumentoValidator.Validar("123456789012")); // 12 dígitos (nem CPF nem CNPJ)
        }

        [TestMethod]
        public void Validar_Vazio_ReturnaFalse()
        {
            Assert.IsFalse(DocumentoValidator.Validar(""));
        }

        // ── Formatação ────────────────────────────────────────────────────────────

        [TestMethod]
        public void FormatarCPF_DigitosNormalizados_FormatadoCorreto()
        {
            Assert.AreEqual(CpfFormatado, DocumentoValidator.FormatarCPF(CpfValido));
        }

        [TestMethod]
        public void FormatarCPF_JaFormatado_FormatadoCorreto()
        {
            Assert.AreEqual(CpfFormatado, DocumentoValidator.FormatarCPF(CpfFormatado));
        }

        [TestMethod]
        public void FormatarCPF_TamanhoErrado_RetornaOriginal()
        {
            Assert.AreEqual("123", DocumentoValidator.FormatarCPF("123"));
        }

        [TestMethod]
        public void FormatarCNPJ_DigitosNormalizados_FormatadoCorreto()
        {
            Assert.AreEqual(CnpjFormatado, DocumentoValidator.FormatarCNPJ(CnpjValido));
        }

        [TestMethod]
        public void FormatarCNPJ_JaFormatado_FormatadoCorreto()
        {
            Assert.AreEqual(CnpjFormatado, DocumentoValidator.FormatarCNPJ(CnpjFormatado));
        }

        [TestMethod]
        public void Formatar_Cpf_RotteiaParaFormatarCPF()
        {
            Assert.AreEqual(CpfFormatado, DocumentoValidator.Formatar(CpfValido));
        }

        [TestMethod]
        public void Formatar_Cnpj_RotteiaParaFormatarCNPJ()
        {
            Assert.AreEqual(CnpjFormatado, DocumentoValidator.Formatar(CnpjValido));
        }

        [TestMethod]
        public void Formatar_TamanhoIncompativel_RetornaOriginal()
        {
            Assert.AreEqual("abc", DocumentoValidator.Formatar("abc"));
        }
    }
}
