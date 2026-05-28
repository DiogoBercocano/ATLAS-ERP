using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Filters;
using ATLAS_ERP.Infrastructure;

namespace ATLAS_ERP.Controllers
{
    [PermissaoFilter("dashboard_view")]
    public class AdminController : Controller
    {
        private readonly AtlasContext db = new AtlasContext();

        public ActionResult Dashboard()
        {
            try
            {
                var empresaIdObj = Session[SessionKeys.EmpresaId] as int?;
                if (!empresaIdObj.HasValue)
                {
                    return RedirectToAction("Login", "Auth");
                }

                int empresaId = empresaIdObj.Value;
                var hoje      = DateTime.Today;

                ViewBag.VendasHoje     = db.Vendas.AsNoTracking().Where(v => v.EmpresaId == empresaId && v.DataVenda >= hoje && v.Status != VendaStatus.Cancelada).Sum(v => (decimal?)v.Total) ?? 0;
                ViewBag.TotalVendasDia = db.Vendas.AsNoTracking().Where(v => v.EmpresaId == empresaId && v.DataVenda >= hoje && v.Status != VendaStatus.Cancelada).Count();
                ViewBag.TotalProdutos  = db.Produtos.AsNoTracking().Where(p => p.EmpresaId == empresaId).Count();
                ViewBag.TotalClientes  = db.Clientes.AsNoTracking().Where(c => c.EmpresaId == empresaId).Count();
                ViewBag.UltimasVendas  = db.Vendas.AsNoTracking().Include(v => v.Cliente).Where(v => v.EmpresaId == empresaId).OrderByDescending(v => v.DataVenda).Take(10).ToList();
                ViewBag.Empresa        = db.Empresas.AsNoTracking().FirstOrDefault(e => e.EmpresaId == empresaId);
                ViewBag.UploadAviso    = TempData["UploadAviso"];
                ViewBag.ConfigSucesso  = TempData["ConfigSucesso"];
                ViewBag.ConfigErro     = TempData["ConfigErro"];

                return View();
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "AdminController.Dashboard");
                return RedirectToAction("Error", "Home");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
