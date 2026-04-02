using System.Data.Entity;
using static System.Data.Entity.Infrastructure.Design.Executor;

namespace ATLAS_ERP.Migrations
{
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

            // ADMIN DA EMPRESA
            context.Usuarios.AddOrUpdate(u => u.Email,
                new Usuario
                {
                    UsuarioId = 1,
                    Name = "Administrador",
                    Email = "admin@atlas.com",
                    SenhaHash = "123",
                    Role = "Admin",
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
                    SenhaHash = "123456",
                    Role = "SuperAdmin",
                    Ativo = true,
                    EmpresaId = null
                }
            );

            context.SaveChanges();
        }
    }
}