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
            try
            {
                if (Session["UsuarioLogado"] == null)
                    return RedirectToAction("Login", "Auth");

                var produtos = db.Produtos.Where(p => p.EmpresaId == EmpresaId).ToList();
                return View(produtos);
            }
            catch
            {
                ViewBag.Erro = "Erro ao carregar produtos.";
                return View(new System.Collections.Generic.List<Produto>());
            }
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
            try
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
            catch
            {
                ViewBag.Erro = "Erro ao cadastrar produto. Tente novamente.";
                return View(produto);
            }
        }

        [HttpPost]
        [RoleFilter("Admin", "Gerente")]
        public ActionResult Edit(int ProdutoId, string Nome, string Categoria, decimal PrecoVenda, int EstoqueMinimo, bool Ativo)
        {
            try
            {
                var p = db.Produtos.FirstOrDefault(x => x.ProdutoId == ProdutoId && x.EmpresaId == EmpresaId);
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
            catch
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [RoleFilter("Admin")]
        public ActionResult Delete(int produtoId)
        {
            try
            {
                var p = db.Produtos.FirstOrDefault(x => x.ProdutoId == produtoId && x.EmpresaId == EmpresaId);
                if (p != null)
                {
                    db.Produtos.Remove(p);
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