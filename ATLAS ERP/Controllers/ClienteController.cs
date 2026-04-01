using System.Linq;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Models;
using ATLAS_ERP.Filters;

namespace ATLAS_ERP.Controllers
{
    public class ClienteController : Controller
    {
        private readonly AtlasContext db = new AtlasContext();

        private int EmpresaId => (int)Session["EmpresaId"];

        public ActionResult Index()
        {
            if (Session["UsuarioLogado"] == null)
                return RedirectToAction("Login", "Auth");

            var clientes = db.Clientes
                             .Where(c => c.EmpresaId == EmpresaId)
                             .ToList();
            return View(clientes);
        }

        [RoleFilter("Admin", "Gerente")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [RoleFilter("Admin", "Gerente")]
        public ActionResult Create(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                cliente.EmpresaId = EmpresaId;
                db.Clientes.Add(cliente);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(cliente);
        }

        [HttpPost]
        [RoleFilter("Admin", "Gerente")]
        public ActionResult Edit(int ClienteId, string Nome, string Documento,
                         string Email, string Telefone, string Endereco,
                         decimal? LimiteCredito, string Ativo)
        {
            var c = db.Clientes
                      .FirstOrDefault(x => x.ClienteId == ClienteId && x.EmpresaId == EmpresaId);
            if (c != null)
            {
                c.Nome = Nome;
                c.Documento = Documento;
                c.Email = Email;
                c.Telefone = Telefone;
                c.Endereco = Endereco;
                c.LimiteCredito = LimiteCredito ?? 0;
                c.Ativo = Ativo == "true";
                db.Entry(c).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [RoleFilter("Admin")]
        public ActionResult Delete(int clienteId)
        {
            var c = db.Clientes
                      .FirstOrDefault(x => x.ClienteId == clienteId && x.EmpresaId == EmpresaId);
            if (c != null)
            {
                db.Clientes.Remove(c);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}