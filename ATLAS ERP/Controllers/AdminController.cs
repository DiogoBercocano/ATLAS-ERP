using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Filters;
using ATLAS_ERP.Infrastructure;

namespace ATLAS_ERP.Controllers
{
    public class AdminController : Controller
    {
        private readonly AtlasContext db = new AtlasContext();

        public ActionResult Dashboard()
        {
            try
            {
                int empresaId = (int)Session[SessionKeys.EmpresaId];
                var hoje      = DateTime.Today;

                ViewBag.VendasHoje     = db.Vendas.AsNoTracking().Where(v => v.EmpresaId == empresaId && v.DataVenda >= hoje && v.Status != VendaStatus.Cancelada).Sum(v => (decimal?)v.Total) ?? 0;
                ViewBag.TotalVendasDia = db.Vendas.AsNoTracking().Where(v => v.EmpresaId == empresaId && v.DataVenda >= hoje && v.Status != VendaStatus.Cancelada).Count();
                ViewBag.TotalProdutos  = db.Produtos.AsNoTracking().Where(p => p.EmpresaId == empresaId).Count();
                ViewBag.TotalClientes  = db.Clientes.AsNoTracking().Where(c => c.EmpresaId == empresaId).Count();
                ViewBag.UltimasVendas  = db.Vendas.AsNoTracking().Include(v => v.Cliente).Where(v => v.EmpresaId == empresaId).OrderByDescending(v => v.DataVenda).Take(10).ToList();
                ViewBag.Empresa        = db.Empresas.AsNoTracking().FirstOrDefault(e => e.EmpresaId == empresaId);

                return View();
            }
            catch (Exception ex)
            {
                Trace.TraceError("AdminController.Dashboard: {0}", ex);
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
