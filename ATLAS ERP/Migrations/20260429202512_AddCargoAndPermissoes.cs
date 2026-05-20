namespace ATLAS_ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddCargoAndPermissoes : DbMigration
    {
        public override void Up()
        {
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
                .ForeignKey("dbo.Empresas", t => t.EmpresaId, cascadeDelete: false)
                .Index(t => t.EmpresaId);

            CreateTable(
                "dbo.CargoPermissaos",
                c => new
                    {
                        CargoId = c.Int(nullable: false),
                        PermissaoId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CargoId, t.PermissaoId })
                .ForeignKey("dbo.Cargos", t => t.CargoId, cascadeDelete: false)
                .ForeignKey("dbo.Permissaos", t => t.PermissaoId, cascadeDelete: false)
                .Index(t => t.CargoId)
                .Index(t => t.PermissaoId);

            AddColumn("dbo.Usuarios", "CargoId", c => c.Int());
            AddForeignKey("dbo.Usuarios", "CargoId", "dbo.Cargos", "CargoId", cascadeDelete: false);
            CreateIndex("dbo.Usuarios", "CargoId");
            DropColumn("dbo.Usuarios", "Role");
        }

        public override void Down()
        {
            AddColumn("dbo.Usuarios", "Role", c => c.String());
            DropForeignKey("dbo.Usuarios", "CargoId", "dbo.Cargos");
            DropIndex("dbo.Usuarios", new[] { "CargoId" });
            DropColumn("dbo.Usuarios", "CargoId");

            DropForeignKey("dbo.CargoPermissaos", "PermissaoId", "dbo.Permissaos");
            DropForeignKey("dbo.CargoPermissaos", "CargoId", "dbo.Cargos");
            DropIndex("dbo.CargoPermissaos", new[] { "PermissaoId" });
            DropIndex("dbo.CargoPermissaos", new[] { "CargoId" });
            DropTable("dbo.CargoPermissaos");

            DropForeignKey("dbo.Cargos", "EmpresaId", "dbo.Empresas");
            DropIndex("dbo.Cargos", new[] { "EmpresaId" });
            DropTable("dbo.Cargos");

            DropTable("dbo.Permissaos");
        }
    }
}
