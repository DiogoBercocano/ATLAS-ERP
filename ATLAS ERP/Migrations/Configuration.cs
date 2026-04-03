namespace ATLAS_ERP.Migrations
{
    using ATLAS_ERP.Helpers;
    using ATLAS_ERP.Models;
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
            if (!context.Usuarios.Any(u => u.Email == "admin@atlas.com"))
            {
                context.Usuarios.Add(new Usuario
                {
                    Name = "Administrador",
                    Email = "admin@atlas.com",
                    SenhaHash = PasswordHelper.Hash("Admin@123"),
                    Role = "Admin",
                    Ativo = true,
                    EmpresaId = 1
                });
            }

            // SUPER ADMIN — sem empresa
            if (!context.Usuarios.Any(u => u.Email == "superadmin@atlas.com"))
            {
                context.Usuarios.Add(new Usuario
                {
                    Name = "Super Admin",
                    Email = "superadmin@atlas.com",
                    SenhaHash = PasswordHelper.Hash("Sadmin@123#654atlas"),
                    Role = "SuperAdmin",
                    Ativo = true,
                    EmpresaId = null
                });
            }

            context.SaveChanges();
        }
    }
}