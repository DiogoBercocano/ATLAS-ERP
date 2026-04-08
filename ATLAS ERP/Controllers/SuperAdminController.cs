using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Filters;
using ATLAS_ERP.Infrastructure;

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
                ViewBag.TotalEmpresas  = db.Empresas.AsNoTracking().Count();
                ViewBag.EmpresasAtivas = db.Empresas.AsNoTracking().Count(e => e.Status == EmpresaStatus.Ativa);
                ViewBag.Pendentes      = db.Empresas.AsNoTracking().Count(e => e.Status == EmpresaStatus.Pendente);
                ViewBag.TotalUsuarios  = db.Usuarios.AsNoTracking().Count();

                var empresas = db.Empresas.AsNoTracking().OrderByDescending(e => e.EmpresaId).ToList();
                return View(empresas);
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("SuperAdminController.Index: {0}", ex);
                return View(new System.Collections.Generic.List<Models.Empresa>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Aprovar(int empresaId)
        {
            try
            {
                var empresa = db.Empresas.Find(empresaId);
                if (empresa != null)
                {
                    empresa.Status = EmpresaStatus.Ativa;
                    empresa.Ativa  = true;
                    db.Entry(empresa).State = EntityState.Modified;

                    foreach (var u in db.Usuarios.Where(u => u.EmpresaId == empresaId))
                    {
                        u.Ativo = true;
                        db.Entry(u).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("SuperAdminController.Aprovar: {0}", ex);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Rejeitar(int empresaId)
        {
            try
            {
                var empresa = db.Empresas.Find(empresaId);
                if (empresa != null)
                {
                    empresa.Status = EmpresaStatus.Pendente;
                    empresa.Ativa  = false;
                    db.Entry(empresa).State = EntityState.Modified;

                    foreach (var u in db.Usuarios.Where(u => u.EmpresaId == empresaId))
                    {
                        u.Ativo = false;
                        db.Entry(u).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("SuperAdminController.Rejeitar: {0}", ex);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Excluir(int empresaId)
        {
            try
            {
                var empresa = db.Empresas.Find(empresaId);
                if (empresa != null)
                {
                    db.Usuarios.RemoveRange(db.Usuarios.Where(u => u.EmpresaId == empresaId));
                    db.Empresas.Remove(empresa);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("SuperAdminController.Excluir: {0}", ex);
                return RedirectToAction("Index");
            }
        }
    }
}
