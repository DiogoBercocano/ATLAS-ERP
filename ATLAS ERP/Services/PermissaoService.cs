using System;
using System.Collections.Generic;
using System.Linq;
using ATLAS_ERP.Data;
using ATLAS_ERP.Models;

namespace ATLAS_ERP.Services
{
    public class PermissaoService
    {
        private readonly AtlasContext _context;

        public PermissaoService(AtlasContext context)
        {
            _context = context;
        }

        public List<Permissao> ListarTodas()
        {
            return _context.Permissoes.OrderBy(p => p.Categoria).ThenBy(p => p.Nome).ToList();
        }

        public List<Permissao> ListarPorCategoria(string categoria)
        {
            return _context.Permissoes
                .Where(p => p.Categoria == categoria)
                .OrderBy(p => p.Nome)
                .ToList();
        }

        public List<string> ListarCategorias()
        {
            return _context.Permissoes
                .Select(p => p.Categoria)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }

        public List<Permissao> ListarPorCargo(int cargoId)
        {
            return _context.CargoPermissoes
                .Where(cp => cp.CargoId == cargoId)
                .Select(cp => cp.Permissao)
                .OrderBy(p => p.Categoria)
                .ThenBy(p => p.Nome)
                .ToList();
        }

        public void CriarPadroes()
        {
            var permissoes = new List<Permissao>
            {
                new Permissao { Chave = "dashboard_view", Nome = "Visualizar Dashboard", Categoria = "Dashboard" },

                new Permissao { Chave = "vendas_view", Nome = "Visualizar Vendas", Categoria = "Vendas" },
                new Permissao { Chave = "vendas_create", Nome = "Criar Vendas", Categoria = "Vendas" },
                new Permissao { Chave = "vendas_edit", Nome = "Editar Vendas", Categoria = "Vendas" },
                new Permissao { Chave = "vendas_delete", Nome = "Deletar Vendas", Categoria = "Vendas" },

                new Permissao { Chave = "produtos_view", Nome = "Visualizar Produtos", Categoria = "Produtos" },
                new Permissao { Chave = "produtos_create", Nome = "Criar Produtos", Categoria = "Produtos" },
                new Permissao { Chave = "produtos_edit", Nome = "Editar Produtos", Categoria = "Produtos" },
                new Permissao { Chave = "produtos_delete", Nome = "Deletar Produtos", Categoria = "Produtos" },

                new Permissao { Chave = "clientes_view", Nome = "Visualizar Clientes", Categoria = "Clientes" },
                new Permissao { Chave = "clientes_create", Nome = "Criar Clientes", Categoria = "Clientes" },
                new Permissao { Chave = "clientes_edit", Nome = "Editar Clientes", Categoria = "Clientes" },
                new Permissao { Chave = "clientes_delete", Nome = "Deletar Clientes", Categoria = "Clientes" },

                new Permissao { Chave = "fornecedores_view", Nome = "Visualizar Fornecedores", Categoria = "Fornecedores" },
                new Permissao { Chave = "fornecedores_create", Nome = "Criar Fornecedores", Categoria = "Fornecedores" },
                new Permissao { Chave = "fornecedores_edit", Nome = "Editar Fornecedores", Categoria = "Fornecedores" },
                new Permissao { Chave = "fornecedores_delete", Nome = "Deletar Fornecedores", Categoria = "Fornecedores" },

                new Permissao { Chave = "usuarios_view", Nome = "Visualizar Funcionários", Categoria = "Funcionários" },
                new Permissao { Chave = "usuarios_create", Nome = "Criar Funcionários", Categoria = "Funcionários" },
                new Permissao { Chave = "usuarios_edit", Nome = "Editar Funcionários", Categoria = "Funcionários" },
                new Permissao { Chave = "usuarios_delete", Nome = "Deletar Funcionários", Categoria = "Funcionários" },

                new Permissao { Chave = "cargos_manage", Nome = "Gerenciar Cargos", Categoria = "Administração" },
                new Permissao { Chave = "settings", Nome = "Configurações", Categoria = "Administração" }
            };

            foreach (var perm in permissoes)
            {
                if (!_context.Permissoes.Any(p => p.Chave == perm.Chave))
                {
                    _context.Permissoes.Add(perm);
                }
            }

            _context.SaveChanges();
        }
    }
}
