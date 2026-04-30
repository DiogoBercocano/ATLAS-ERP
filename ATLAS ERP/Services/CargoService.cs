using System;
using System.Collections.Generic;
using System.Linq;
using ATLAS_ERP.Data;
using ATLAS_ERP.Models;

namespace ATLAS_ERP.Services
{
    public class CargoService
    {
        private readonly AtlasContext _context;

        public CargoService(AtlasContext context)
        {
            _context = context;
        }

        public List<Cargo> ListarPorEmpresa(int empresaId)
        {
            return _context.Cargos
                .Where(c => c.EmpresaId == empresaId && c.Ativo)
                .ToList();
        }

        public Cargo BuscarPorId(int cargoId, int empresaId)
        {
            return _context.Cargos
                .FirstOrDefault(c => c.CargoId == cargoId && c.EmpresaId == empresaId);
        }

        public void Criar(Cargo cargo)
        {
            _context.Cargos.Add(cargo);
            _context.SaveChanges();
        }

        public void Editar(int cargoId, int empresaId, string nome, string descricao)
        {
            var cargo = BuscarPorId(cargoId, empresaId);
            if (cargo != null)
            {
                cargo.Nome = nome;
                cargo.Descricao = descricao;
                _context.SaveChanges();
            }
        }

        public void Excluir(int cargoId, int empresaId)
        {
            var cargo = BuscarPorId(cargoId, empresaId);
            if (cargo != null)
            {
                cargo.Ativo = false;
                _context.SaveChanges();
            }
        }

        public void AdicionarPermissao(int cargoId, int permissaoId)
        {
            var jaExiste = _context.CargoPermissoes
                .Any(cp => cp.CargoId == cargoId && cp.PermissaoId == permissaoId);

            if (!jaExiste)
            {
                var cargoPermissao = new CargoPermissao
                {
                    CargoId = cargoId,
                    PermissaoId = permissaoId
                };
                _context.CargoPermissoes.Add(cargoPermissao);
                _context.SaveChanges();
            }
        }

        public void RemoverPermissao(int cargoId, int permissaoId)
        {
            var cargoPermissao = _context.CargoPermissoes
                .FirstOrDefault(cp => cp.CargoId == cargoId && cp.PermissaoId == permissaoId);

            if (cargoPermissao != null)
            {
                _context.CargoPermissoes.Remove(cargoPermissao);
                _context.SaveChanges();
            }
        }
    }
}
