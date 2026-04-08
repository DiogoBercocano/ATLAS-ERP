namespace ATLAS_ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddEstoqueAtualRemoverFilial : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Filials", "EmpresaId", "dbo.Empresas");
            DropIndex("dbo.Filials", new[] { "EmpresaId" });
            AddColumn("dbo.Produtoes", "EstoqueAtual", c => c.Int(nullable: false));
            DropTable("dbo.Filials");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Filials",
                c => new
                    {
                        FilialId = c.Int(nullable: false, identity: true),
                        Nome = c.String(nullable: false),
                        Cidadde = c.String(),
                        Estado = c.String(),
                        EmpresaId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.FilialId);
            
            DropColumn("dbo.Produtoes", "EstoqueAtual");
            CreateIndex("dbo.Filials", "EmpresaId");
            AddForeignKey("dbo.Filials", "EmpresaId", "dbo.Empresas", "EmpresaId");
        }
    }
}
