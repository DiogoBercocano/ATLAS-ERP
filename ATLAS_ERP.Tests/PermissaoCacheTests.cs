using System.Threading;
using System.Web;
using ATLAS_ERP.Infrastructure;
using Moq;

namespace ATLAS_ERP.Tests
{
    [TestClass]
    public class PermissaoCacheTests
    {
        // ── GlobalVersion / InvalidarGlobal ──────────────────────────────────────

        [TestMethod]
        public void InvalidarGlobal_IncrementaVersao()
        {
            var antes = PermissaoCache.GlobalVersion;
            PermissaoCache.InvalidarGlobal();
            Assert.AreEqual(antes + 1, PermissaoCache.GlobalVersion);
        }

        [TestMethod]
        public void InvalidarGlobal_MultiplasChamadas_IncrementaCorretamente()
        {
            var antes = PermissaoCache.GlobalVersion;
            PermissaoCache.InvalidarGlobal();
            PermissaoCache.InvalidarGlobal();
            PermissaoCache.InvalidarGlobal();
            Assert.AreEqual(antes + 3, PermissaoCache.GlobalVersion);
        }

        [TestMethod]
        public void GlobalVersion_LeituraConsequente_RetornaMesmoValor()
        {
            // Sem incremento entre leituras, o valor não muda
            var v1 = PermissaoCache.GlobalVersion;
            var v2 = PermissaoCache.GlobalVersion;
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void InvalidarGlobal_Concorrente_NaoCorrompeContador()
        {
            // 10 threads cada uma chama 10 vezes → total = 100 incrementos
            var antes = PermissaoCache.GlobalVersion;
            var threads = new Thread[10];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() =>
                {
                    for (int j = 0; j < 10; j++)
                        PermissaoCache.InvalidarGlobal();
                });
            }
            foreach (var t in threads) t.Start();
            foreach (var t in threads) t.Join();

            Assert.AreEqual(antes + 100, PermissaoCache.GlobalVersion);
        }

        // ── InvalidarSessao(HttpSessionStateBase) ────────────────────────────────

        [TestMethod]
        public void InvalidarSessao_ComSessao_RemoveChavesDePermissao()
        {
            var session = new Mock<HttpSessionStateBase>();
            PermissaoCache.InvalidarSessao(session.Object);

            session.Verify(s => s.Remove("PermissoesSet"),      Times.Once);
            session.Verify(s => s.Remove("PermissoesSetVersao"), Times.Once);
        }

        [TestMethod]
        public void InvalidarSessao_SessaoNull_NaoLanca()
        {
            // Deve tolerar null sem lançar exceção
            PermissaoCache.InvalidarSessao((HttpSessionStateBase)null!);
        }

        [TestMethod]
        public void InvalidarSessao_NaoRemoveOutrasChaves()
        {
            var session = new Mock<HttpSessionStateBase>(MockBehavior.Strict);
            session.Setup(s => s.Remove("PermissoesSet"));
            session.Setup(s => s.Remove("PermissoesSetVersao"));

            // MockBehavior.Strict lança se qualquer outra chamada ocorrer
            PermissaoCache.InvalidarSessao(session.Object);
            session.VerifyAll();
        }
    }
}
