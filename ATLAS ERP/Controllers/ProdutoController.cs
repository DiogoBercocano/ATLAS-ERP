using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Filters;
using ATLAS_ERP.Models;
using ATLAS_ERP.Services;

namespace ATLAS_ERP.Controllers
{
    public class ProdutoController : Controller
    {
        private readonly ProdutoService _service;
        private int EmpresaId => (int)Session[Infrastructure.SessionKeys.EmpresaId];

        public ProdutoController()
        {
            _service = new ProdutoService(new AtlasContext());
        }

        [PermissaoFilter("produtos_view")]
        public ActionResult Index()
        {
            if (Session[Infrastructure.SessionKeys.UsuarioLogado] == null)
                return RedirectToAction("Login", "Auth");
            try
            {
                return View(_service.ListarPorEmpresa(EmpresaId));
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("ProdutoController.Index: {0}", ex);
                ViewBag.Erro = "Erro ao carregar produtos.";
                return View(new System.Collections.Generic.List<Produto>());
            }
        }

        [PermissaoFilter("produtos_create")]
        public ActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissaoFilter("produtos_create")]
        public ActionResult Create(Produto produto, HttpPostedFileBase foto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    produto.EmpresaId = EmpresaId;
                    produto.FotoUrl   = SalvarFoto(foto);
                    _service.Criar(produto);
                    return RedirectToAction("Index");
                }
                return View(produto);
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("ProdutoController.Create: {0}", ex);
                ViewBag.Erro = "Erro ao cadastrar produto. Tente novamente.";
                return View(produto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissaoFilter("produtos_edit")]
        public ActionResult Edit(int ProdutoId, string Nome, string Categoria,
                                 string PrecoVenda, int EstoqueMinimo, bool Ativo,
                                 HttpPostedFileBase foto)
        {
            try
            {
                decimal.TryParse(PrecoVenda,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal preco);

                var novaFoto = SalvarFoto(foto);
                if (novaFoto != null)
                    DeletarFoto(_service.BuscarPorId(ProdutoId, EmpresaId)?.FotoUrl);

                _service.Editar(ProdutoId, EmpresaId, Nome, Categoria, preco, EstoqueMinimo, Ativo, novaFoto);
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("ProdutoController.Edit: {0}", ex);
                return RedirectToAction("Index");
            }
        }

        private void DeletarFoto(string fotoUrl)
        {
            if (string.IsNullOrEmpty(fotoUrl)) return;
            try
            {
                var path = Server.MapPath("~" + fotoUrl);
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }
            catch { }
        }

        private string SalvarFoto(HttpPostedFileBase foto)
        {
            if (foto == null || foto.ContentLength == 0) return null;

            var exts = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var ext  = Path.GetExtension(foto.FileName).ToLowerInvariant();
            if (System.Array.IndexOf(exts, ext) < 0) return null;

            var pasta = Server.MapPath("~/Content/images/produtos");
            if (!Directory.Exists(pasta)) Directory.CreateDirectory(pasta);

            var nomeArquivo = System.Guid.NewGuid().ToString("N") + ext;
            foto.SaveAs(Path.Combine(pasta, nomeArquivo));

            return "/Content/images/produtos/" + nomeArquivo;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissaoFilter("produtos_delete")]
        public ActionResult Delete(int produtoId)
        {
            try
            {
                _service.Excluir(produtoId, EmpresaId);
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("ProdutoController.Delete: {0}", ex);
                return RedirectToAction("Index");
            }
        }
    }
}
