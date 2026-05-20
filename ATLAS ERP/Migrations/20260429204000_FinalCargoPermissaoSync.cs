namespace ATLAS_ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class FinalCargoPermissaoSync : DbMigration
    {
        public override void Up()
        {
            // Verificar se as tabelas já existem antes de criar
            // A tabela CargoPermissoes pode precisar ser recriada com a chave correta

            try
            {
                DropForeignKey("dbo.CargoPermissaos", "CargoId", "dbo.Cargos");
            }
            catch { }

            try
            {
                DropForeignKey("dbo.CargoPermissaos", "PermissaoId", "dbo.Permissaos");
            }
            catch { }

            try
            {
                DropIndex("dbo.CargoPermissaos", new[] { "CargoId" });
            }
            catch { }

            try
            {
                DropIndex("dbo.CargoPermissaos", new[] { "PermissaoId" });
            }
            catch { }

            try
            {
                DropTable("dbo.CargoPermissaos");
            }
            catch { }

            // Recriar a tabela com a chave primária correta
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
        }

        public override void Down()
        {
            DropForeignKey("dbo.CargoPermissaos", "PermissaoId", "dbo.Permissaos");
            DropForeignKey("dbo.CargoPermissaos", "CargoId", "dbo.Cargos");
            DropIndex("dbo.CargoPermissaos", new[] { "PermissaoId" });
            DropIndex("dbo.CargoPermissaos", new[] { "CargoId" });
            DropTable("dbo.CargoPermissaos");
        }
    }
}
