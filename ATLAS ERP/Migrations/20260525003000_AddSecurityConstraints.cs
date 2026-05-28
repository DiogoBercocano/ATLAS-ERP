namespace ATLAS_ERP.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddSecurityConstraints : DbMigration
    {
        public override void Up()
        {
            // Enforce unique email per empresa (multi-tenant: same email allowed across different companies)
            Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Usuario_Email_Empresa' AND object_id = OBJECT_ID('dbo.Usuarios'))
                  CREATE UNIQUE INDEX IX_Usuario_Email_Empresa ON dbo.Usuarios(EmpresaId, Email)");

            // Flag to force password change on first login (e.g., for seed/default accounts)
            Sql(@"IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Usuarios') AND name = 'MustChangePassword')
                  ALTER TABLE dbo.Usuarios ADD MustChangePassword BIT NOT NULL DEFAULT 0");
        }

        public override void Down()
        {
            Sql(@"IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Usuario_Email_Empresa' AND object_id = OBJECT_ID('dbo.Usuarios'))
                  DROP INDEX IX_Usuario_Email_Empresa ON dbo.Usuarios");

            Sql(@"IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Usuarios') AND name = 'MustChangePassword')
                  ALTER TABLE dbo.Usuarios DROP COLUMN MustChangePassword");
        }
    }
}
