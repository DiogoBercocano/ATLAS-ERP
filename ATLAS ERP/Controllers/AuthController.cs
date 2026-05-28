using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Helpers;
using ATLAS_ERP.Infrastructure;

namespace ATLAS_ERP.Controllers
{
    public class AuthController : Controller
    {
        private readonly AtlasContext db = new AtlasContext();

        public ActionResult Login()
        {
            try
            {
                if (Session[SessionKeys.UsuarioLogado] != null)
                {
                    var cargoNome = Session[SessionKeys.CargoNome]?.ToString();
                    if (cargoNome == "SuperAdmin")
                        return RedirectToAction("Index", "SuperAdmin");
                    if (cargoNome == "Admin" || cargoNome == "Gerente")
                        return RedirectToAction("Dashboard", "Admin");
                    if (!string.IsNullOrEmpty(cargoNome))
                        return RedirectToAction("Index", "Produto");
                    // Cargo nulo/desconhecido: evita loop infinito forçando logout
                    Session.Clear();
                    Session.Abandon();
                }
                return View();
            }
            catch (System.Exception ex)
            {
                AppLogger.Error(ex, "AuthController.Login GET");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string senha)
        {
            var ip = Request?.UserHostAddress;

            try
            {
                var bloqueio = LoginRateLimiter.VerificarBloqueio(ip, email);
                if (bloqueio.Bloqueado)
                {
                    AppLogger.Audit("login_bloqueado motivo={0} email={1} ip={2} segundos={3}",
                                    bloqueio.Motivo, email, ip,
                                    (int)bloqueio.TempoRestante.TotalSeconds);
                    ViewBag.Erro = "Muitas tentativas de login. Tente novamente em "
                                   + FormatarEspera(bloqueio.TempoRestante) + ".";
                    return View();
                }

                var user = db.Usuarios
                    .Include(u => u.Cargo)
                    .FirstOrDefault(u => u.Email == email && u.Ativo == true);

                if (user == null || !PasswordHelper.Verificar(senha, user.SenhaHash))
                {
                    var falha = LoginRateLimiter.RegistrarFalha(ip, email);
                    AppLogger.Audit("login_falhou email={0} ip={1}", email, ip);

                    if (falha.Bloqueado)
                    {
                        AppLogger.Audit("login_lockout motivo={0} email={1} ip={2} segundos={3}",
                                        falha.Motivo, email, ip,
                                        (int)falha.TempoRestante.TotalSeconds);
                        ViewBag.Erro = "Muitas tentativas de login. Tente novamente em "
                                       + FormatarEspera(falha.TempoRestante) + ".";
                    }
                    else
                    {
                        ViewBag.Erro = "E-mail ou senha inválidos.";
                    }
                    return View();
                }

                LoginRateLimiter.RegistrarSucesso(ip, email);

                if (PasswordHelper.PrecisaRehash(user.SenhaHash))
                {
                    user.SenhaHash = PasswordHelper.HashSenha(senha);
                    db.SaveChanges();
                    AppLogger.Audit("rehash_senha usuarioId={0}", user.UsuarioId);
                }

                AppLogger.Audit("login_sucesso usuarioId={0} email={1} cargo={2}",
                                user.UsuarioId, user.Email, user.Cargo?.Nome);

                // Usuário sem cargo: conta incompleta, não pode prosseguir
                if (user.Cargo == null)
                {
                    AppLogger.Audit("login_sem_cargo usuarioId={0} email={1}", user.UsuarioId, user.Email);
                    ViewBag.Erro = "Conta sem perfil de acesso configurado. Contate o administrador do sistema.";
                    return View();
                }

                // Anti session-poisoning: descarta qualquer state pré-login antes de promover
                // a sessão ao usuário autenticado. SessionID em si não é regenerado nesta request
                // (constraint do ASP.NET clássico sem Identity middleware — ver SECURITY.md),
                // mas qualquer chave injetada pré-auth via XSS/CSRF é apagada aqui.
                Session.Clear();
                PermissaoCache.InvalidarSessao(Session);

                Session[SessionKeys.UsuarioLogado] = user.Name;
                Session[SessionKeys.UsuarioId]     = user.UsuarioId;
                Session[SessionKeys.CargoId]       = user.CargoId;
                Session[SessionKeys.CargoNome]     = user.Cargo.Nome;
                Session[SessionKeys.EmpresaId]     = user.EmpresaId.HasValue ? (object)user.EmpresaId.Value : null;

                if (user.MustChangePassword)
                {
                    Session[SessionKeys.MustChangePassword] = true;
                    return RedirectToAction("AlterarSenhaInicial");
                }

                var cargoNome = user.Cargo.Nome;
                if (cargoNome == "SuperAdmin")
                    return RedirectToAction("Index", "SuperAdmin");
                if (cargoNome == "Admin" || cargoNome == "Gerente")
                    return RedirectToAction("Dashboard", "Admin");

                return RedirectToAction("Index", "Produto");
            }
            catch (System.Exception ex)
            {
                AppLogger.Error(ex, "AuthController.Login POST");
                ViewBag.Erro = "Erro ao realizar login. Tente novamente.";
                return View();
            }
        }

        [HttpGet]
        public ActionResult AlterarSenhaInicial()
        {
            if (Session[SessionKeys.UsuarioLogado] == null)
                return RedirectToAction("Login");
            if (!(Session[SessionKeys.MustChangePassword] is true))
                return RedirectToAction("Login");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AlterarSenhaInicial(string novaSenha, string confirmar)
        {
            if (Session[SessionKeys.UsuarioLogado] == null)
                return RedirectToAction("Login");
            if (!(Session[SessionKeys.MustChangePassword] is true))
                return RedirectToAction("Login");

            if (string.IsNullOrWhiteSpace(novaSenha) || novaSenha.Length < 8)
            {
                ViewBag.Erro = "A senha deve ter pelo menos 8 caracteres.";
                return View();
            }
            if (novaSenha != confirmar)
            {
                ViewBag.Erro = "As senhas não coincidem.";
                return View();
            }

            try
            {
                var usuarioId = (int)Session[SessionKeys.UsuarioId];
                var user = db.Usuarios.Find(usuarioId);
                if (user == null) return RedirectToAction("Login");

                user.SenhaHash = PasswordHelper.HashSenha(novaSenha);
                user.MustChangePassword = false;
                db.SaveChanges();

                Session.Remove(SessionKeys.MustChangePassword);
                AppLogger.Audit("senha_inicial_alterada usuarioId={0}", usuarioId);

                var cargoNome = Session[SessionKeys.CargoNome]?.ToString();
                if (cargoNome == "SuperAdmin")
                    return RedirectToAction("Index", "SuperAdmin");
                if (cargoNome == "Admin" || cargoNome == "Gerente")
                    return RedirectToAction("Dashboard", "Admin");

                return RedirectToAction("Index", "Produto");
            }
            catch (System.Exception ex)
            {
                AppLogger.Error(ex, "AuthController.AlterarSenhaInicial POST");
                ViewBag.Erro = "Erro ao alterar senha. Tente novamente.";
                return View();
            }
        }

        public ActionResult Logout()
        {
            var usuarioId = Session[SessionKeys.UsuarioId];
            AppLogger.Audit("logout usuarioId={0}", usuarioId);

            PermissaoCache.InvalidarSessao(Session);
            Session.Clear();
            Session.Abandon();

            // Força browser a soltar os cookies de sessão e antiforgery. Sem isso, o cookie
            // permanece válido até o timeout natural e poderia ser replayed se interceptado.
            ExpirarCookie("ATLAS.SID");
            ExpirarCookie("ATLAS.AFT");

            return RedirectToAction("Login");
        }

        private void ExpirarCookie(string nome)
        {
            if (Request?.Cookies[nome] == null) return;

            var cookie = new System.Web.HttpCookie(nome, string.Empty)
            {
                Expires  = DateTime.UtcNow.AddYears(-1),
                HttpOnly = true,
                Secure   = Request.IsSecureConnection,
                Path     = "/"
            };
            Response.Cookies.Add(cookie);
        }

        private static string FormatarEspera(TimeSpan ts)
        {
            if (ts.TotalMinutes >= 1)
            {
                var minutos = (int)Math.Ceiling(ts.TotalMinutes);
                return minutos + (minutos == 1 ? " minuto" : " minutos");
            }
            var segundos = Math.Max(1, (int)Math.Ceiling(ts.TotalSeconds));
            return segundos + (segundos == 1 ? " segundo" : " segundos");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db?.Dispose();
            base.Dispose(disposing);
        }
    }
}
