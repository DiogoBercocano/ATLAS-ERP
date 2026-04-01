namespace ATLAS_ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLogoPathEmpresa : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Empresas", "LogoPath", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Empresas", "LogoPath");
        }
    }
}
