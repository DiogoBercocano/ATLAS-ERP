using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Filters;

namespace ATLAS_ERP.Controllers
{
    [RoleFilter("SuperAdmin")]
    public class SuperAdminController : Controller
    {
        private readonly AtlasContext db = new AtlasContext();

        public ActionResult Index()
        {
            try
            {
                ViewBag.TotalEmpresas = db.Empresas.Count();
                ViewBag.EmpresasAtivas = db.Empresas.Count(e => e.Status == "Ativa");
                ViewBag.Pendentes = db.Empresas.Count(e => e.Status == "Pendente");
                ViewBag.TotalUsuarios = db.Usuarios.Count();

                var empresas = db.Empresas.OrderByDescending(e => e.EmpresaId).ToList();
                return View(empresas);
            }
            catch
            {
                return View(new System.Collections.Generic.List<ATLAS_ERP.Models.Empresa>());
            }
        }

        [HttpPost]
        public ActionResult Aprovar(int empresaId)
        {
            try
            {
                var empresa = db.Empresas.Find(empresaId);
                if (empresa != null)
                {
                    empresa.Status = "Ativa";
                    empresa.Ativa = true;
                    db.Entry(empresa).State = EntityState.Modified;

                    var usuarios = db.Usuarios.Where(u => u.EmpresaId == empresaId).ToList();
                    foreach (var u in usuarios)
                    {
                        u.Ativo = true;
                        db.Entry(u).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Rejeitar(int empresaId)
        {
            try
            {
                var empresa = db.Empresas.Find(empresaId);
                if (empresa != null)
                {
                    empresa.Status = "Pendente";
                    empresa.Ativa = false;
                    db.Entry(empresa).State = EntityState.Modified;

                    var usuarios = db.Usuarios.Where(u => u.EmpresaId == empresaId).ToList();
                    foreach (var u in usuarios)
                    {
                        u.Ativo = false;
                        db.Entry(u).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Excluir(int empresaId)
        {
            try
            {
                var empresa = db.Empresas.Find(empresaId);
                if (empresa != null)
                {
                    var usuarios = db.Usuarios.Where(u => u.EmpresaId == empresaId).ToList();
                    db.Usuarios.RemoveRange(usuarios);
                    db.Empresas.Remove(empresa);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }
    }
}