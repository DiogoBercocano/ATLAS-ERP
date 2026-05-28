namespace ATLAS_ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCargoPermissaoSystem : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CargoPermissaos",
                c => new
                    {
                        CargoId = c.Int(nullable: false),
                        PermissaoId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CargoId, t.PermissaoId })
                .ForeignKey("dbo.Cargos", t => t.CargoId)
                .ForeignKey("dbo.Permissaos", t => t.PermissaoId)
                .Index(t => t.CargoId)
                .Index(t => t.PermissaoId);
            
            CreateTable(
                "dbo.Cargos",
                c => new
                    {
                        CargoId = c.Int(nullable: false, identity: true),
                        Nome = c.String(nullable: false, maxLength: 100),
                        Descricao = c.String(maxLength: 500),
                        Ativo = c.Boolean(nullable: false),
                        EmpresaId = c.Int(nullable: false),
                        DataCriacao = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.CargoId)
                .ForeignKey("dbo.Empresas", t => t.EmpresaId)
                .Index(t => t.EmpresaId);
            
            CreateTable(
                "dbo.Permissaos",
                c => new
                    {
                        PermissaoId = c.Int(nullable: false, identity: true),
                        Chave = c.String(nullable: false, maxLength: 100),
                        Nome = c.String(nullable: false, maxLength: 200),
                        Descricao = c.String(maxLength: 500),
                        Categoria = c.String(),
                    })
                .PrimaryKey(t => t.PermissaoId);
            
            AddColumn("dbo.Usuarios", "CargoId", c => c.Int());
            AddColumn("dbo.Usuarios", "MustChangePassword", c => c.Boolean(nullable: false));
            CreateIndex("dbo.Usuarios", "CargoId");
            AddForeignKey("dbo.Usuarios", "CargoId", "dbo.Cargos", "CargoId");
            DropColumn("dbo.Usuarios", "Role");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Usuarios", "Role", c => c.String());
            DropForeignKey("dbo.CargoPermissaos", "PermissaoId", "dbo.Permissaos");
            DropForeignKey("dbo.CargoPermissaos", "CargoId", "dbo.Cargos");
            DropForeignKey("dbo.Cargos", "EmpresaId", "dbo.Empresas");
            DropForeignKey("dbo.Usuarios", "CargoId", "dbo.Cargos");
            DropIndex("dbo.Usuarios", new[] { "CargoId" });
            DropIndex("dbo.Cargos", new[] { "EmpresaId" });
            DropIndex("dbo.CargoPermissaos", new[] { "PermissaoId" });
            DropIndex("dbo.CargoPermissaos", new[] { "CargoId" });
            DropColumn("dbo.Usuarios", "MustChangePassword");
            DropColumn("dbo.Usuarios", "CargoId");
            DropTable("dbo.Permissaos");
            DropTable("dbo.Cargos");
            DropTable("dbo.CargoPermissaos");
        }
    }
}
