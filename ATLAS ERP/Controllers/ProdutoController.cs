using System.IO;
using System.Web;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Filters;
using ATLAS_ERP.Infrastructure;
using ATLAS_ERP.Models;
using ATLAS_ERP.Services;

namespace ATLAS_ERP.Controllers
{
    public class ProdutoController : Controller
    {
        private readonly AtlasContext   _db;
        private readonly ProdutoService _service;
        private int EmpresaId => (int)Session[Infrastructure.SessionKeys.EmpresaId];

        public ProdutoController()
        {
            _db      = new AtlasContext();
            _service = new ProdutoService(_db);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db?.Dispose();
            base.Dispose(disposing);
        }

        [PermissaoFilter("produtos_view")]
        public ActionResult Index(int page = 1, int pageSize = PagingDefaults.PageSize, string search = null)
        {
            if (Session[Infrastructure.SessionKeys.UsuarioLogado] == null)
                return RedirectToAction("Login", "Auth");
            try
            {
                return View(_service.ListarPaginado(EmpresaId, page, pageSize, search));
            }
            catch (System.Exception ex)
            {
                AppLogger.Error(ex, "ProdutoController.Index");
                ViewBag.Erro = "Erro ao carregar produtos.";
                return View(PagedResult<Produto>.Empty(pageSize));
            }
        }

        [PermissaoFilter("produtos_create")]
        public ActionResult Create() => View();

        [PermissaoFilter("produtos_edit")]
        public ActionResult Edit(int id)
        {
            var produto = _service.BuscarPorId(id, EmpresaId);
            if (produto == null)
            {
                TempData["Erro"] = "Produto não encontrado.";
                return RedirectToAction("Index");
            }
            return View(produto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissaoFilter("produtos_create")]
        public ActionResult Create(Produto produto, HttpPostedFileBase foto)
        {
            try
            {
                decimal.TryParse(Request.Form["PrecoVenda"],
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal preco);
                produto.PrecoVenda = preco;
                ModelState.Remove("PrecoVenda");
                if (preco <= 0)
                    ModelState.AddModelError("PrecoVenda", "Informe um preço de venda válido.");

                if (ModelState.IsValid)
                {
                    produto.EmpresaId = EmpresaId;
                    string fotoUrl; bool fotoRejeitada;
                    SalvarFoto(foto, out fotoUrl, out fotoRejeitada);
                    produto.FotoUrl = fotoUrl;
                    _service.Criar(produto);
                    if (fotoRejeitada)
                        TempData["UploadAviso"] = "Produto salvo, mas a imagem foi rejeitada. Use JPG, PNG ou WebP com até 5 MB e sem pontos extras no nome.";
                    return RedirectToAction("Index");
                }
                return View(produto);
            }
            catch (System.Exception ex)
            {
                AppLogger.Error(ex, "ProdutoController.Create");
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

                string novaFoto; bool fotoRejeitada;
                SalvarFoto(foto, out novaFoto, out fotoRejeitada);
                if (novaFoto != null)
                    DeletarFoto(_service.BuscarPorId(ProdutoId, EmpresaId)?.FotoUrl);

                _service.Editar(ProdutoId, EmpresaId, Nome, Categoria, preco, EstoqueMinimo, Ativo, novaFoto);
                if (fotoRejeitada)
                    TempData["UploadAviso"] = "Produto salvo, mas a imagem foi rejeitada. Use JPG, PNG ou WebP com até 5 MB e sem pontos extras no nome.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                AppLogger.Error(ex, "ProdutoController.Edit");
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

        private void SalvarFoto(HttpPostedFileBase foto, out string fotoUrl, out bool rejeitada)
        {
            fotoUrl   = null;
            rejeitada = false;

            var usuarioId = Session[Infrastructure.SessionKeys.UsuarioId] as int?;
            var empresaId = Session[Infrastructure.SessionKeys.EmpresaId] as int?;

            var resultado = UploadValidator.Validar(foto, contexto: "produto_foto");
            resultado.Auditar(usuarioId, empresaId);

            if (resultado.Vazio)    return;
            if (!resultado.Sucesso) { rejeitada = true; return; }

            var pastaFisica   = Server.MapPath("~/Content/images/produtos");
            var pastaRelativa = "/Content/images/produtos";
            fotoUrl = UploadValidator.Persistir(foto, resultado, pastaFisica, pastaRelativa);
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
                AppLogger.Error(ex, "ProdutoController.Delete");
                return RedirectToAction("Index");
            }
        }
    }
}
