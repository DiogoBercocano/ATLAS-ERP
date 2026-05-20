namespace ATLAS_ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFotoProduto : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Produtoes", "FotoUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Produtoes", "FotoUrl");
        }
    }
}
