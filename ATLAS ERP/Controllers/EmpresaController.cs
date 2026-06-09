using System;
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
                var cnpjLimpo = DocumentoValidator.Normalizar(CNPJ);

                if (string.IsNullOrWhiteSpace(NomeEmpresa))           { ViewBag.Erro = "Informe a Razão Social da empresa.";    return View(); }
                if (cnpjLimpo.Length != 14)                           { ViewBag.Erro = "CNPJ inválido. Digite os 14 dígitos.";  return View(); }
                if (!DocumentoValidator.ValidarCNPJ(cnpjLimpo))       { ViewBag.Erro = "CNPJ inválido (dígitos verificadores não conferem)."; return View(); }
                if (string.IsNullOrWhiteSpace(NomeAdmin))             { ViewBag.Erro = "Informe o nome do administrador.";      return View(); }
                if (string.IsNullOrWhiteSpace(EmailAdmin))            { ViewBag.Erro = "Informe o e-mail de acesso.";           return View(); }
                if (string.IsNullOrWhiteSpace(Senha))                 { ViewBag.Erro = "Informe uma senha.";                    return View(); }

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

                // Cria cargo "Admin" padrão para a empresa com todas as permissões
                var cargoAdmin = new Cargo
                {
                    Nome        = "Admin",
                    Descricao   = "Administrador da empresa",
                    Ativo       = true,
                    EmpresaId   = empresa.EmpresaId,
                    DataCriacao = DateTime.Now
                };
                db.Cargos.Add(cargoAdmin);
                db.SaveChanges();

                foreach (var perm in db.Permissoes.ToList())
                {
                    db.CargoPermissoes.Add(new CargoPermissao
                    {
                        CargoId     = cargoAdmin.CargoId,
                        PermissaoId = perm.PermissaoId
                    });
                }

                db.Usuarios.Add(new Usuario
                {
                    Name              = NomeAdmin,
                    Email             = EmailAdmin,
                    SenhaHash         = PasswordHelper.HashSenha(Senha),
                    Ativo             = false,
                    EmpresaId         = empresa.EmpresaId,
                    CargoId           = cargoAdmin.CargoId,
                    MustChangePassword = true
                });
                db.SaveChanges();

                ViewBag.Sucesso = "Cadastro enviado com sucesso! Aguarde a aprovação do administrador.";
                return View();
            }
            catch (System.Exception ex)
            {
                AppLogger.Error(ex, "EmpresaController.Cadastro");
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

                    var usuarioId = Session[SessionKeys.UsuarioId] as int?;
                    var resultado = UploadValidator.Validar(Logo, contexto: "empresa_logo",
                                                            maxBytes: UploadValidator.LogoMaxBytes);
                    resultado.Auditar(usuarioId, empresaId);

                    if (resultado.Sucesso)
                    {
                        var pastaFisica   = Server.MapPath("~/Content/images/logos");
                        var pastaRelativa = "/Content/images/logos";
                        var urlPublica    = UploadValidator.Persistir(Logo, resultado, pastaFisica, pastaRelativa);

                        // Persiste apenas o nome do arquivo (compat com schema existente);
                        // urlPublica vem como "/Content/images/logos/<nome>".
                        empresa.LogoPath = resultado.NomeFinal;
                    }
                    else if (!resultado.Vazio)
                    {
                        TempData["UploadAviso"] = "Logo rejeitado: use JPG, PNG ou WebP com até 2 MB.";
                    }

                    db.Entry(empresa).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                if (TempData["UploadAviso"] == null && TempData["ConfigErro"] == null)
                    TempData["ConfigSucesso"] = "Configurações salvas com sucesso!";
                return RedirectToAction("Dashboard", "Admin");
            }
            catch (System.Exception ex)
            {
                AppLogger.Error(ex, "EmpresaController.Configuracoes");
                TempData["ConfigErro"] = "Erro ao salvar configurações. Tente novamente.";
                return RedirectToAction("Dashboard", "Admin");
            }
        }
    }
}
