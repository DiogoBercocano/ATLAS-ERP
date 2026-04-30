using System.Data.Entity;
using static System.Data.Entity.Infrastructure.Design.Executor;

namespace ATLAS_ERP.Migrations
{
    using ATLAS_ERP.Helpers;
    using ATLAS_ERP.Models;
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ATLAS_ERP.Data.AtlasContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ATLAS_ERP.Data.AtlasContext context)
        {
            // EMPRESA PADRÃO
            context.Empresas.AddOrUpdate(e => e.CNPJ,
                new Empresa
                {
                    EmpresaId = 1,
                    Nome = "Atlas ERP",
                    CNPJ = "00000000000000",
                    Email = "contato@atlas.com",
                    Ativa = true,
                    Status = "Ativa"
                }
            );
            context.SaveChanges();

            // CRIAR PERMISSÕES PADRÃO
            var permissoes = new[]
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
                if (!context.Permissoes.Any(p => p.Chave == perm.Chave))
                {
                    context.Permissoes.Add(perm);
                }
            }
            context.SaveChanges();

            // CRIAR CARGOS PADRÃO
            context.Cargos.AddOrUpdate(c => new { c.Nome, c.EmpresaId },
                new Cargo
                {
                    CargoId = 1,
                    Nome = "Admin",
                    Descricao = "Administrador da empresa",
                    Ativo = true,
                    EmpresaId = 1,
                    DataCriacao = DateTime.Now
                },
                new Cargo
                {
                    CargoId = 2,
                    Nome = "Gerente",
                    Descricao = "Gerente da empresa",
                    Ativo = true,
                    EmpresaId = 1,
                    DataCriacao = DateTime.Now
                }
            );
            context.SaveChanges();

            // ATRIBUIR TODAS AS PERMISSÕES AO ADMIN
            var adminCargo = context.Cargos.FirstOrDefault(c => c.Nome == "Admin");
            if (adminCargo != null)
            {
                var permissoesAdmin = context.Permissoes.ToList();
                foreach (var perm in permissoesAdmin)
                {
                    if (!context.CargoPermissoes.Any(cp => cp.CargoId == adminCargo.CargoId && cp.PermissaoId == perm.PermissaoId))
                    {
                        context.CargoPermissoes.Add(new CargoPermissao
                        {
                            CargoId = adminCargo.CargoId,
                            PermissaoId = perm.PermissaoId
                        });
                    }
                }
                context.SaveChanges();
            }

            // CRIAR CARGO SUPERADMIN (SEM EMPRESA)
            if (!context.Cargos.Any(c => c.Nome == "SuperAdmin"))
            {
                context.Cargos.Add(new Cargo
                {
                    CargoId = 3,
                    Nome = "SuperAdmin",
                    Descricao = "Super Administrador do sistema",
                    Ativo = true,
                    EmpresaId = 1,
                    DataCriacao = DateTime.Now
                });
                context.SaveChanges();
            }

            // ADMIN DA EMPRESA
            context.Usuarios.AddOrUpdate(u => u.Email,
                new Usuario
                {
                    UsuarioId = 1,
                    Name = "Administrador",
                    Email = "admin@atlas.com",
                    SenhaHash = PasswordHelper.HashSenha("Atlas@2026"),
                    CargoId = 1,
                    Ativo = true,
                    EmpresaId = 1
                }
            );

            // SUPER ADMIN — sem empresa
            context.Usuarios.AddOrUpdate(u => u.Email,
                new Usuario
                {
                    UsuarioId = 2,
                    Name = "Super Admin",
                    Email = "superadmin@atlas.com",
                    SenhaHash = PasswordHelper.HashSenha("SuperAtlas@2026"),
                    CargoId = 3,
                    Ativo = true,
                    EmpresaId = null
                }
            );

            context.SaveChanges();
        }
    }
}