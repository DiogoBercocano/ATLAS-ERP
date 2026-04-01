namespace ATLAS_ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStatusEmpresa : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Empresas", "Status", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Empresas", "Status");
        }
    }
}
