namespace ATLAS_ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using ATLAS_ERP.Models;

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
                new ATLAS_ERP.Models.Empresa
                {
                    EmpresaId = 1,
                    Nome = "Atlas ERP",
                    CNPJ = "00000000000000",
                    Email = "contato@atlas.com",
                    Ativa = true
                }
            );

            context.SaveChanges();

            // USUÁRIO ADMIN
            context.Usuarios.AddOrUpdate(u => u.Email,
                new ATLAS_ERP.Models.Usuario
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

            context.SaveChanges();
        }
    }
}