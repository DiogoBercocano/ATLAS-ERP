using System;
using System.Diagnostics;
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

                var produtos = db.Produtos.Where(p => p.EmpresaId == empresaId.Value).ToList();
                return View(produtos);
            }
            catch (Exception ex)
            {
                Trace.TraceError("[ProdutoController.Index] {0}", ex);
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
                var empresaId = EmpresaIdOrNull;
                if (empresaId == null)
                    return RedirectToAction("Login", "Auth");

                if (ModelState.IsValid)
                {
                    produto.EmpresaId = empresaId.Value;
                    db.Produtos.Add(produto);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(produto);
            }
            catch (Exception ex)
            {
                Trace.TraceError("[ProdutoController.Create POST] {0}", ex);
                ViewBag.Erro = "Erro ao cadastrar produto. Tente novamente.";
                return View(produto);
            }
        }

        [HttpPost]
        [RoleFilter("Admin", "Gerente")]
        public ActionResult Edit(int produtoId, string Nome, string Categoria, decimal PrecoVenda, int EstoqueMinimo, bool Ativo)
        {
            try
            {
                var empresaId = EmpresaIdOrNull;
                if (empresaId == null)
                    return RedirectToAction("Login", "Auth");

                if (PrecoVenda < 0) PrecoVenda = 0;
                if (EstoqueMinimo < 0) EstoqueMinimo = 0;

                var p = db.Produtos.FirstOrDefault(x => x.ProdutoId == produtoId && x.EmpresaId == empresaId.Value);
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
            catch (Exception ex)
            {
                Trace.TraceError("[ProdutoController.Edit POST] {0}", ex);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [RoleFilter("Admin")]
        public ActionResult Delete(int produtoId)
        {
            try
            {
                var empresaId = EmpresaIdOrNull;
                if (empresaId == null)
                    return RedirectToAction("Login", "Auth");

                var p = db.Produtos.FirstOrDefault(x => x.ProdutoId == produtoId && x.EmpresaId == empresaId.Value);
                if (p != null)
                {
                    db.Produtos.Remove(p);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Trace.TraceError("[ProdutoController.Delete POST] {0}", ex);
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
