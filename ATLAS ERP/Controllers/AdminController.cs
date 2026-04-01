using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Filters;

namespace ATLAS_ERP.Controllers
{
    public class AdminController : Controller
    {
        private readonly AtlasContext db = new AtlasContext();

        [RoleFilter("Admin", "Gerente")]
        public ActionResult Dashboard()
        {
            int empresaId = (int)Session["EmpresaId"];
            var hoje = DateTime.Today;

            ViewBag.VendasHoje = db.Vendas
                                       .Where(v => v.EmpresaId == empresaId && v.DataVenda >= hoje)
                                       .Sum(v => (decimal?)v.Total) ?? 0;

            ViewBag.TotalVendasDia = db.Vendas
                                       .Where(v => v.EmpresaId == empresaId && v.DataVenda >= hoje)
                                       .Count();

            ViewBag.TotalProdutos = db.Produtos
                                       .Where(p => p.EmpresaId == empresaId)
                                       .Count();

            ViewBag.TotalClientes = db.Clientes
                                       .Where(c => c.EmpresaId == empresaId)
                                       .Count();

            ViewBag.UltimasVendas = db.Vendas
                                       .Include(v => v.Cliente)
                                       .Where(v => v.EmpresaId == empresaId)
                                       .OrderByDescending(v => v.DataVenda)
                                       .Take(10)
                                       .ToList();

            ViewBag.Empresa = db.Empresas.Find(empresaId);

            return View();
        }
    }
}