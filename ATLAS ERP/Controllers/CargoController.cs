using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using ATLAS_ERP.Data;
using ATLAS_ERP.Filters;
using ATLAS_ERP.Infrastructure;
using ATLAS_ERP.Models;
using ATLAS_ERP.Services;

namespace ATLAS_ERP.Controllers
{
    [PermissaoFilter("cargos_manage")]
    public class CargoController : Controller
    {
        private readonly AtlasContext db = new AtlasContext();
        private readonly CargoService _cargoService;
        private readonly PermissaoService _permissaoService;
        private int EmpresaId => (int)Session[Infrastructure.SessionKeys.EmpresaId];

        public CargoController()
        {
            _cargoService = new CargoService(db);
            _permissaoService = new PermissaoService(db);
        }

        public ActionResult Index()
        {
            try
            {
                var cargos = _cargoService.ListarPorEmpresa(EmpresaId)
                    .Where(c => c.Nome != "SuperAdmin")
                    .ToList();
                return View(cargos);
            }
            catch (Exception ex)
            {
                Trace.TraceError("CargoController.Index: {0}", ex);
                ViewBag.Erro = $"Erro ao carregar cargos: {ex.Message}";
                return View(new List<Cargo>());
            }
        }

        public ActionResult Create()
        {
            return View(new Cargo());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Cargo cargo)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    cargo.EmpresaId = EmpresaId;
                    cargo.DataCriacao = DateTime.Now;
                    _cargoService.Criar(cargo);
                    return RedirectToAction("Index");
                }
                return View(cargo);
            }
            catch (Exception ex)
            {
                Trace.TraceError("CargoController.Create POST: {0}", ex);
                ViewBag.Erro = $"Erro ao criar cargo: {ex.Message}";
                return View(cargo);
            }
        }

        public ActionResult Edit(int id)
        {
            try
            {
                var cargo = _cargoService.BuscarPorId(id, EmpresaId);
                if (cargo == null || cargo.Nome == "SuperAdmin")
                    return RedirectToAction("Index");

                return View(cargo);
            }
            catch (Exception ex)
            {
                Trace.TraceError("CargoController.Edit GET: {0}", ex);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, string nome, string descricao)
        {
            try
            {
                _cargoService.Editar(id, EmpresaId, nome, descricao);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Trace.TraceError("CargoController.Edit POST: {0}", ex);
                ViewBag.Erro = $"Erro ao editar cargo: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        public ActionResult Permissoes(int id)
        {
            try
            {
                var cargo = _cargoService.BuscarPorId(id, EmpresaId);
                if (cargo == null || cargo.Nome == "SuperAdmin")
                    return RedirectToAction("Index");

                _permissaoService.CriarPadroes();
                var permissoesCargo = _permissaoService.ListarPorCargo(id);
                var todasPermissoes = _permissaoService.ListarTodas();

                ViewBag.Cargo = cargo;
                ViewBag.PermissoesCargo = permissoesCargo;
                ViewBag.TodasPermissoes = todasPermissoes;
                ViewBag.PermissaoPorCategoria = todasPermissoes.GroupBy(p => p.Categoria)
                    .ToDictionary(g => g.Key, g => g.ToList());

                return View(cargo);
            }
            catch (Exception ex)
            {
                Trace.TraceError("CargoController.Permissoes: {0}", ex);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdicionarPermissao(int cargoId, int permissaoId)
        {
            try
            {
                var cargo = _cargoService.BuscarPorId(cargoId, EmpresaId);
                if (cargo != null)
                {
                    _cargoService.AdicionarPermissao(cargoId, permissaoId);
                    PermissaoCache.InvalidarGlobal();
                    AppLogger.Audit("permissao_adicionada cargoId={0} permissaoId={1}", cargoId, permissaoId);
                }
                return RedirectToAction("Permissoes", new { id = cargoId });
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "CargoController.AdicionarPermissao cargoId={0} permissaoId={1}", cargoId, permissaoId);
                return RedirectToAction("Permissoes", new { id = cargoId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoverPermissao(int cargoId, int permissaoId)
        {
            try
            {
                var cargo = _cargoService.BuscarPorId(cargoId, EmpresaId);
                if (cargo != null)
                {
                    _cargoService.RemoverPermissao(cargoId, permissaoId);
                    PermissaoCache.InvalidarGlobal();
                    AppLogger.Audit("permissao_removida cargoId={0} permissaoId={1}", cargoId, permissaoId);
                }
                return RedirectToAction("Permissoes", new { id = cargoId });
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "CargoController.RemoverPermissao cargoId={0} permissaoId={1}", cargoId, permissaoId);
                return RedirectToAction("Permissoes", new { id = cargoId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                var cargo = _cargoService.BuscarPorId(id, EmpresaId);
                if (cargo == null || cargo.Nome == "SuperAdmin")
                    return RedirectToAction("Index");

                _cargoService.Excluir(id, EmpresaId);
                PermissaoCache.InvalidarGlobal();
                AppLogger.Audit("cargo_excluido cargoId={0} nome={1}", id, cargo.Nome);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "CargoController.Delete id={0}", id);
                return RedirectToAction("Index");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db?.Dispose();
            base.Dispose(disposing);
        }
    }
}
