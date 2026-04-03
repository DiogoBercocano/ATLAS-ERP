using System;
using System.Diagnostics;
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

        private int? EmpresaIdOrNull => Session["EmpresaId"] as int?;

        public ActionResult Index()
        {
            try
            {
                if (Session["UsuarioLogado"] == null)
                    return RedirectToAction("Login", "Auth");

                var empresaId = EmpresaIdOrNull;
                if (empresaId == null)
                    return RedirectToAction("Login", "Auth");

                var clientes = db.Clientes.Where(c => c.EmpresaId == empresaId.Value).ToList();
                return View(clientes);
            }
            catch (Exception ex)
            {
                Trace.TraceError("[ClienteController.Index] {0}", ex);
                ViewBag.Erro = "Erro ao carregar clientes.";
                return View(new System.Collections.Generic.List<Cliente>());
            }
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
            try
            {
                var empresaId = EmpresaIdOrNull;
                if (empresaId == null)
                    return RedirectToAction("Login", "Auth");

                if (ModelState.IsValid)
                {
                    cliente.EmpresaId = empresaId.Value;
                    db.Clientes.Add(cliente);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(cliente);
            }
            catch (Exception ex)
            {
                Trace.TraceError("[ClienteController.Create POST] {0}", ex);
                ViewBag.Erro = "Erro ao cadastrar cliente. Tente novamente.";
                return View(cliente);
            }
        }

        [HttpPost]
        [RoleFilter("Admin", "Gerente")]
        public ActionResult Edit(int clienteId, string Nome, string Documento,
                                 string Email, string Telefone, string Endereco,
                                 decimal? LimiteCredito, bool Ativo)
        {
            try
            {
                var empresaId = EmpresaIdOrNull;
                if (empresaId == null)
                    return RedirectToAction("Login", "Auth");

                var c = db.Clientes.FirstOrDefault(x => x.ClienteId == clienteId && x.EmpresaId == empresaId.Value);
                if (c != null)
                {
                    c.Nome = Nome;
                    c.Documento = Documento;
                    c.Email = Email;
                    c.Telefone = Telefone;
                    c.Endereco = Endereco;
                    c.LimiteCredito = LimiteCredito.HasValue && LimiteCredito.Value >= 0 ? LimiteCredito.Value : 0;
                    c.Ativo = Ativo;
                    db.Entry(c).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Trace.TraceError("[ClienteController.Edit POST] {0}", ex);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [RoleFilter("Admin")]
        public ActionResult Delete(int clienteId)
        {
            try
            {
                var empresaId = EmpresaIdOrNull;
                if (empresaId == null)
                    return RedirectToAction("Login", "Auth");

                var c = db.Clientes.FirstOrDefault(x => x.ClienteId == clienteId && x.EmpresaId == empresaId.Value);
                if (c != null)
                {
                    db.Clientes.Remove(c);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Trace.TraceError("[ClienteController.Delete POST] {0}", ex);
                return RedirectToAction("Index");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
