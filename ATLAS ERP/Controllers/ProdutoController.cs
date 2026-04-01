using System.Linq;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Models;
using ATLAS_ERP.Filters;

namespace ATLAS_ERP.Controllers
{
    public class ProdutoController : Controller
    {
        private readonly AtlasContext db = new AtlasContext();

        private int EmpresaId => (int)Session["EmpresaId"];

        public ActionResult Index()
        {
            if (Session["UsuarioLogado"] == null)
                return RedirectToAction("Login", "Auth");

            var produtos = db.Produtos
                             .Where(p => p.EmpresaId == EmpresaId)
                             .ToList();
            return View(produtos);
        }

        [RoleFilter("Admin", "Gerente")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [RoleFilter("Admin", "Gerente")]
        public ActionResult Create(Produto produto)
        {
            if (ModelState.IsValid)
            {
                produto.EmpresaId = EmpresaId;
                db.Produtos.Add(produto);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(produto);
        }

        [HttpPost]
        [RoleFilter("Admin", "Gerente")]
        public ActionResult Edit(int ProdutoId, string Nome, string Categoria, decimal PrecoVenda, int EstoqueMinimo, bool Ativo)
        {
            var p = db.Produtos
                      .FirstOrDefault(x => x.ProdutoId == ProdutoId && x.EmpresaId == EmpresaId);
            if (p != null)
            {
                p.Nome = Nome;
                p.Categoria = Categoria;
                p.PrecoVenda = PrecoVenda;
                p.EstoqueMinimo = EstoqueMinimo;
                p.Ativo = Ativo;
                db.Entry(p).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [RoleFilter("Admin")]
        public ActionResult Delete(int produtoId)
        {
            var p = db.Produtos
                      .FirstOrDefault(x => x.ProdutoId == produtoId && x.EmpresaId == EmpresaId);
            if (p != null)
            {
                db.Produtos.Remove(p);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}