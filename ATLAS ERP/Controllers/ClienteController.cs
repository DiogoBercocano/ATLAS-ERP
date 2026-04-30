using System.Diagnostics;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Filters;
using ATLAS_ERP.Models;
using ATLAS_ERP.Services;

namespace ATLAS_ERP.Controllers
{
    public class ClienteController : Controller
    {
        private readonly ClienteService _service;
        private int EmpresaId => (int)Session[Infrastructure.SessionKeys.EmpresaId];

        public ClienteController()
        {
            _service = new ClienteService(new AtlasContext());
        }

        [PermissaoFilter("clientes_view")]
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
                Trace.TraceError("ClienteController.Index: {0}", ex);
                ViewBag.Erro = "Erro ao carregar clientes.";
                return View(new System.Collections.Generic.List<Cliente>());
            }
        }

        [PermissaoFilter("clientes_create")]
        public ActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissaoFilter("clientes_create")]
        public ActionResult Create(Cliente cliente)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    cliente.EmpresaId = EmpresaId;
                    _service.Criar(cliente);
                    return RedirectToAction("Index");
                }
                return View(cliente);
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("ClienteController.Create: {0}", ex);
                ViewBag.Erro = "Erro ao cadastrar cliente. Tente novamente.";
                return View(cliente);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissaoFilter("clientes_edit")]
        public ActionResult Edit(int ClienteId, string Nome, string Documento,
                                 string Email, string Telefone, string Endereco,
                                 decimal? LimiteCredito, string Ativo)
        {
            try
            {
                _service.Editar(ClienteId, EmpresaId, Nome, Documento, Email,
                                Telefone, Endereco, LimiteCredito ?? 0, Ativo == "true");
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("ClienteController.Edit: {0}", ex);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissaoFilter("clientes_delete")]
        public ActionResult Delete(int clienteId)
        {
            try
            {
                _service.Excluir(clienteId, EmpresaId);
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                Trace.TraceError("ClienteController.Delete: {0}", ex);
                return RedirectToAction("Index");
            }
        }
    }
}
