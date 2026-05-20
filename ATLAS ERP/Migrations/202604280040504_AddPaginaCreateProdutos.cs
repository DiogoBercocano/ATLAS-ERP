namespace ATLAS_ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPaginaCreateProdutos : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Empresas", "Telefone", c => c.String());
            AddColumn("dbo.Empresas", "Endereco", c => c.String());
            AddColumn("dbo.Fornecedors", "Endereco", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Fornecedors", "Endereco");
            DropColumn("dbo.Empresas", "Endereco");
            DropColumn("dbo.Empresas", "Telefone");
        }
    }
}
