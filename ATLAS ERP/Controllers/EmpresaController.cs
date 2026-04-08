using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Filters;
using ATLAS_ERP.Helpers;
using ATLAS_ERP.Infrastructure;
using ATLAS_ERP.Models;

namespace ATLAS_ERP.Controllers
{
    public class EmpresaController : Controller
    {
        private readonly AtlasContext db = new AtlasContext();

        [AllowAnonymous]
        public ActionResult Cadastro() => View();

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cadastro(string NomeEmpresa, string CNPJ, string EmailEmpresa,
                                     string NomeAdmin, string EmailAdmin, string Senha)
        {
            try
            {
                var cnpjLimpo = string.IsNullOrEmpty(CNPJ)
                    ? ""
                    : new string(CNPJ.Where(char.IsDigit).ToArray());

                if (string.IsNullOrWhiteSpace(NomeEmpresa))  { ViewBag.Erro = "Informe a Razão Social da empresa.";    return View(); }
                if (cnpjLimpo.Length != 14)                  { ViewBag.Erro = "CNPJ inválido. Digite os 14 dígitos.";  return View(); }
                if (string.IsNullOrWhiteSpace(NomeAdmin))    { ViewBag.Erro = "Informe o nome do administrador.";      return View(); }
                if (string.IsNullOrWhiteSpace(EmailAdmin))   { ViewBag.Erro = "Informe o e-mail de acesso.";           return View(); }
                if (string.IsNullOrWhiteSpace(Senha))        { ViewBag.Erro = "Informe uma senha.";                    return View(); }

                if (db.Empresas.Any(e => e.CNPJ == cnpjLimpo))       { ViewBag.Erro = "CNPJ já cadastrado no sistema.";               return View(); }
                if (db.Usuarios.Any(u => u.Email == EmailAdmin))      { ViewBag.Erro = "E-mail de administrador já cadastrado.";        return View(); }

                var empresa = new Empresa
                {
                    Nome   = NomeEmpresa,
                    CNPJ   = cnpjLimpo,
                    Email  = EmailEmpresa,
                    Ativa  = false,
                    Status = EmpresaStatus.Pendente
                };

                db.Empresas.Add(empresa);
                db.SaveChanges();

                db.Usuarios.Add(new Usuario
                {
                    Name       = NomeAdmin,
                    Email      = EmailAdmin,
                    SenhaHash  = PasswordHelper.HashSenha(Senha),
                    Role       = Roles.Admin,
                    Ativo      = false,
                    EmpresaId  = empresa.EmpresaId
                });
                db.SaveChanges();

                ViewBag.Sucesso = "Cadastro enviado com sucesso! Aguarde a aprovação do administrador.";
                return View();
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("EmpresaController.Cadastro: {0}", ex);
                ViewBag.Erro = "Erro ao processar cadastro. Tente novamente.";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleFilter("Admin")]
        public ActionResult Configuracoes(string Email, HttpPostedFileBase Logo)
        {
            try
            {
                int empresaId = (int)Session[SessionKeys.EmpresaId];
                var empresa   = db.Empresas.Find(empresaId);

                if (empresa != null)
                {
                    if (!string.IsNullOrEmpty(Email))
                        empresa.Email = Email;

                    if (Logo != null && Logo.ContentLength > 0)
                    {
                        var ext      = System.IO.Path.GetExtension(Logo.FileName);
                        var fileName = "logo_" + empresaId + ext;
                        var path     = System.Web.HttpContext.Current.Server.MapPath("~/Content/images/logos/" + fileName);
                        Logo.SaveAs(path);
                        empresa.LogoPath = fileName;
                    }

                    db.Entry(empresa).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                TempData["ConfigSucesso"] = "Configurações salvas com sucesso!";
                return RedirectToAction("Dashboard", "Admin");
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("EmpresaController.Configuracoes: {0}", ex);
                return RedirectToAction("Dashboard", "Admin");
            }
        }
    }
}
