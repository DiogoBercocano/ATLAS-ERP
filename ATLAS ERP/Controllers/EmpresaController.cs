using ATLAS_ERP.Data;
using ATLAS_ERP.Filters;
using ATLAS_ERP.Models;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ATLAS_ERP.Controllers
{
    public class EmpresaController : Controller
    {
        private readonly AtlasContext db = new AtlasContext();

        // TELA PUBLICA DE CADASTRO - GET
        [AllowAnonymous]
        public ActionResult Cadastro()
        {
            return View();
        }

        // TELA PUBLICA DE CADASTRO - POST
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Cadastro(string NomeEmpresa, string CNPJ, string EmailEmpresa,
                                     string NomeAdmin, string EmailAdmin, string Senha)
        {
            // verifica se CNPJ já existe
            if (db.Empresas.Any(e => e.CNPJ == CNPJ))
            {
                ViewBag.Erro = "CNPJ já cadastrado no sistema.";
                return View();
            }

            // verifica se email admin já existe
            if (db.Usuarios.Any(u => u.Email == EmailAdmin))
            {
                ViewBag.Erro = "E-mail de administrador já cadastrado.";
                return View();
            }

            // cria empresa como pendente
            var empresa = new Empresa
            {
                Nome = NomeEmpresa,
                CNPJ = CNPJ,
                Email = EmailEmpresa,
                Ativa = false,
                Status = "Pendente"
            };

            db.Empresas.Add(empresa);
            db.SaveChanges();

            // cria usuário admin vinculado — inativo até aprovação
            var admin = new Usuario
            {
                Name = NomeAdmin,
                Email = EmailAdmin,
                SenhaHash = Senha,
                Role = "Admin",
                Ativo = false,
                EmpresaId = empresa.EmpresaId
            };

            db.Usuarios.Add(admin);
            db.SaveChanges();

            ViewBag.Sucesso = "Cadastro enviado! Aguarde a aprovação do administrador.";
            return View();
        }

        // GET — retorna dados da empresa para o ViewBag do Dashboard
        // (chamado pelo AdminController, não precisa de action separada)

        // POST — salvar configurações
        [HttpPost]
        [RoleFilter("Admin")]
        public ActionResult Configuracoes(string Email, HttpPostedFileBase Logo)
        {
            int empresaId = (int)Session["EmpresaId"];
            var empresa = db.Empresas.Find(empresaId);

            if (empresa != null)
            {
                if (!string.IsNullOrEmpty(Email))
                    empresa.Email = Email;

                // salvar logo
                if (Logo != null && Logo.ContentLength > 0)
                {
                    var ext = System.IO.Path.GetExtension(Logo.FileName);
                    var fileName = "logo_" + empresaId + ext;
                    var path = System.Web.HttpContext.Current.Server.MapPath("~/Content/images/logos/" + fileName);
                    Logo.SaveAs(path);
                    empresa.LogoPath = fileName;
                }

                db.Entry(empresa).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }

            TempData["ConfigSucesso"] = "Configurações salvas com sucesso!";
            return RedirectToAction("Dashboard", "Admin");
        }
    }
}