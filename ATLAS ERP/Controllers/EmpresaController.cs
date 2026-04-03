using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Filters;
using ATLAS_ERP.Helpers;
using ATLAS_ERP.Models;

namespace ATLAS_ERP.Controllers
{
    public class EmpresaController : Controller
    {
        private readonly AtlasContext db = new AtlasContext();

        private static readonly string[] ExtensoesPemitidasLogo = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const int TamanhoMaximoLogoBytes = 2 * 1024 * 1024; // 2 MB

        [AllowAnonymous]
        public ActionResult Cadastro()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Cadastro(string NomeEmpresa, string CNPJ, string EmailEmpresa,
                                     string NomeAdmin, string EmailAdmin, string Senha)
        {
            try
            {
                var cnpjLimpo = string.IsNullOrEmpty(CNPJ)
                    ? ""
                    : new string(CNPJ.Where(char.IsDigit).ToArray());

                if (string.IsNullOrWhiteSpace(NomeEmpresa)) { ViewBag.Erro = "Informe a Razão Social da empresa."; return View(); }
                if (cnpjLimpo.Length != 14) { ViewBag.Erro = "CNPJ inválido. Digite os 14 dígitos."; return View(); }
                if (string.IsNullOrWhiteSpace(NomeAdmin)) { ViewBag.Erro = "Informe o nome do administrador."; return View(); }
                if (string.IsNullOrWhiteSpace(EmailAdmin)) { ViewBag.Erro = "Informe o e-mail de acesso."; return View(); }
                if (string.IsNullOrWhiteSpace(Senha) || Senha.Length < 6) { ViewBag.Erro = "A senha deve ter ao menos 6 caracteres."; return View(); }

                if (db.Empresas.Any(e => e.CNPJ == cnpjLimpo)) { ViewBag.Erro = "CNPJ já cadastrado no sistema."; return View(); }
                if (db.Usuarios.Any(u => u.Email == EmailAdmin)) { ViewBag.Erro = "E-mail de administrador já cadastrado."; return View(); }

                var empresa = new Empresa
                {
                    Nome = NomeEmpresa,
                    CNPJ = cnpjLimpo,
                    Email = EmailEmpresa,
                    Ativa = false,
                    Status = "Pendente"
                };

                db.Empresas.Add(empresa);
                db.SaveChanges();

                var admin = new Usuario
                {
                    Name = NomeAdmin,
                    Email = EmailAdmin,
                    SenhaHash = PasswordHelper.Hash(Senha),
                    Role = "Admin",
                    Ativo = false,
                    EmpresaId = empresa.EmpresaId
                };

                db.Usuarios.Add(admin);
                db.SaveChanges();

                ViewBag.Sucesso = "Cadastro enviado com sucesso! Aguarde a aprovação do administrador.";
                return View();
            }
            catch (Exception ex)
            {
                Trace.TraceError("[EmpresaController.Cadastro POST] {0}", ex);
                ViewBag.Erro = "Erro ao processar cadastro. Tente novamente.";
                return View();
            }
        }

        [HttpPost]
        [RoleFilter("Admin")]
        public ActionResult Configuracoes(string Email, HttpPostedFileBase Logo)
        {
            try
            {
                if (Session["EmpresaId"] == null)
                    return RedirectToAction("Login", "Auth");

                int empresaId = (int)Session["EmpresaId"];
                var empresa = db.Empresas.Find(empresaId);

                if (empresa != null)
                {
                    if (!string.IsNullOrEmpty(Email))
                        empresa.Email = Email;

                    if (Logo != null && Logo.ContentLength > 0)
                    {
                        if (Logo.ContentLength > TamanhoMaximoLogoBytes)
                        {
                            TempData["ConfigErro"] = "O arquivo de logo não pode ultrapassar 2 MB.";
                            return RedirectToAction("Dashboard", "Admin");
                        }

                        var ext = Path.GetExtension(Logo.FileName)?.ToLowerInvariant();
                        if (string.IsNullOrEmpty(ext) || Array.IndexOf(ExtensoesPemitidasLogo, ext) < 0)
                        {
                            TempData["ConfigErro"] = "Formato de imagem inválido. Use JPG, PNG, GIF ou WEBP.";
                            return RedirectToAction("Dashboard", "Admin");
                        }

                        var fileName = "logo_" + empresaId + ext;
                        var path = HttpContext.Server.MapPath("~/Content/images/logos/" + fileName);
                        Logo.SaveAs(path);
                        empresa.LogoPath = fileName;
                    }

                    db.Entry(empresa).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                TempData["ConfigSucesso"] = "Configurações salvas com sucesso!";
                return RedirectToAction("Dashboard", "Admin");
            }
            catch (Exception ex)
            {
                Trace.TraceError("[EmpresaController.Configuracoes POST] {0}", ex);
                TempData["ConfigErro"] = "Erro ao salvar configurações. Tente novamente.";
                return RedirectToAction("Dashboard", "Admin");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
