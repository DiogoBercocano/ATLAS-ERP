using System;
using System.Collections.Generic;
using ATLAS_ERP.Infrastructure;
using ATLAS_ERP.Services;

namespace ATLAS_ERP.Tests
{
    /// <summary>
    /// Testa a camada de validação de VendaService.
    /// Todos os cenários exercitam ValidarEntradaCriacao(), que é chamada ANTES de qualquer
    /// acesso ao banco — por isso o DbContext pode ser null nestes testes.
    /// </summary>
    [TestClass]
    public class VendaServiceValidacaoTests
    {
        // Vencimento válido = hoje ou futuro
        private static readonly DateTime VencimentoValido  = DateTime.Today.AddDays(1);
        private static readonly DateTime VencimentoPassado = DateTime.Today.AddDays(-1);

        private static VendaService CriarService() => new VendaService(null!);

        private static List<VendaItemDto> ItensValidos() => new List<VendaItemDto>
        {
            new VendaItemDto { ProdutoId = 1, Quantidade = 2, Preco = 50m }
        };

        // ── Criar — validação de itens ────────────────────────────────────────────

        [TestMethod]
        [ExpectedException(typeof(DomainException))]
        public void Criar_SemItens_ThrowsDomainException()
        {
            CriarService().Criar(1, 1, 1, VencimentoValido, new List<VendaItemDto>());
        }

        [TestMethod]
        [ExpectedException(typeof(DomainException))]
        public void Criar_ItensNull_ThrowsDomainException()
        {
            CriarService().Criar(1, 1, 1, VencimentoValido, null!);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainException))]
        public void Criar_VencimentoPassado_ThrowsDomainException()
        {
            CriarService().Criar(1, 1, 1, VencimentoPassado, ItensValidos());
        }

        [TestMethod]
        [ExpectedException(typeof(DomainException))]
        public void Criar_QuantidadeZero_ThrowsDomainException()
        {
            var itens = new List<VendaItemDto>
            {
                new VendaItemDto { ProdutoId = 1, Quantidade = 0, Preco = 50m }
            };
            CriarService().Criar(1, 1, 1, VencimentoValido, itens);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainException))]
        public void Criar_QuantidadeNegativa_ThrowsDomainException()
        {
            var itens = new List<VendaItemDto>
            {
                new VendaItemDto { ProdutoId = 1, Quantidade = -1, Preco = 50m }
            };
            CriarService().Criar(1, 1, 1, VencimentoValido, itens);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainException))]
        public void Criar_PrecoNegativo_ThrowsDomainException()
        {
            var itens = new List<VendaItemDto>
            {
                new VendaItemDto { ProdutoId = 1, Quantidade = 1, Preco = -0.01m }
            };
            CriarService().Criar(1, 1, 1, VencimentoValido, itens);
        }

        [TestMethod]
        public void Criar_PrecoZero_NaoThrows()
        {
            // Preço zero é permitido (produto brinde, desconto total, etc.)
            var itens = new List<VendaItemDto>
            {
                new VendaItemDto { ProdutoId = 1, Quantidade = 1, Preco = 0m }
            };
            // Deve passar a validação e lançar NullReferenceException ao tentar acessar o DB
            // (não DomainException — a validação foi bem-sucedida)
            Assert.ThrowsException<NullReferenceException>(
                () => CriarService().Criar(1, 1, 1, VencimentoValido, itens));
        }

        [TestMethod]
        public void Criar_VencimentoHoje_NaoThrowsDomainException()
        {
            // Vencimento = hoje deve ser aceito pela validação
            Assert.ThrowsException<NullReferenceException>(
                () => CriarService().Criar(1, 1, 1, DateTime.Today, ItensValidos()));
        }

        // ── Editar — mesmas regras de validação ───────────────────────────────────

        [TestMethod]
        [ExpectedException(typeof(DomainException))]
        public void Editar_SemItens_ThrowsDomainException()
        {
            CriarService().Editar(1, 1, 1, VencimentoValido, new List<VendaItemDto>());
        }

        [TestMethod]
        [ExpectedException(typeof(DomainException))]
        public void Editar_ItensNull_ThrowsDomainException()
        {
            CriarService().Editar(1, 1, 1, VencimentoValido, null!);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainException))]
        public void Editar_VencimentoPassado_ThrowsDomainException()
        {
            CriarService().Editar(1, 1, 1, VencimentoPassado, ItensValidos());
        }

        [TestMethod]
        [ExpectedException(typeof(DomainException))]
        public void Editar_QuantidadeZero_ThrowsDomainException()
        {
            var itens = new List<VendaItemDto>
            {
                new VendaItemDto { ProdutoId = 1, Quantidade = 0, Preco = 50m }
            };
            CriarService().Editar(1, 1, 1, VencimentoValido, itens);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainException))]
        public void Editar_PrecoNegativo_ThrowsDomainException()
        {
            var itens = new List<VendaItemDto>
            {
                new VendaItemDto { ProdutoId = 1, Quantidade = 1, Preco = -1m }
            };
            CriarService().Editar(1, 1, 1, VencimentoValido, itens);
        }

        // ── Mensagens de erro ─────────────────────────────────────────────────────

        [TestMethod]
        public void Criar_SemItens_MensagemCorreta()
        {
            var ex = Assert.ThrowsException<DomainException>(
                () => CriarService().Criar(1, 1, 1, VencimentoValido, new List<VendaItemDto>()));
            StringAssert.Contains(ex.Message, "item");
        }

        [TestMethod]
        public void Criar_VencimentoPassado_MensagemCorreta()
        {
            var ex = Assert.ThrowsException<DomainException>(
                () => CriarService().Criar(1, 1, 1, VencimentoPassado, ItensValidos()));
            StringAssert.Contains(ex.Message, "vencimento");
        }

        [TestMethod]
        public void Criar_QuantidadeZero_MensagemCorreta()
        {
            var itens = new List<VendaItemDto>
            {
                new VendaItemDto { ProdutoId = 1, Quantidade = 0, Preco = 10m }
            };
            var ex = Assert.ThrowsException<DomainException>(
                () => CriarService().Criar(1, 1, 1, VencimentoValido, itens));
            StringAssert.Contains(ex.Message.ToLower(), "quantidade");
        }
    }
}
