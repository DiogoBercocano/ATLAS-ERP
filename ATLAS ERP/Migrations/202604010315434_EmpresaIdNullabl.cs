namespace ATLAS_ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EmpresaIdNullabl : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Usuarios", new[] { "EmpresaId" });
            AlterColumn("dbo.Usuarios", "EmpresaId", c => c.Int());
            CreateIndex("dbo.Usuarios", "EmpresaId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Usuarios", new[] { "EmpresaId" });
            AlterColumn("dbo.Usuarios", "EmpresaId", c => c.Int(nullable: false));
            CreateIndex("dbo.Usuarios", "EmpresaId");
        }
    }
}
