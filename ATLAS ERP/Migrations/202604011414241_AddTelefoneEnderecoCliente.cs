namespace ATLAS_ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTelefoneEnderecoCliente : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Clientes", "Telefone", c => c.String());
            AddColumn("dbo.Clientes", "Endereco", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Clientes", "Endereco");
            DropColumn("dbo.Clientes", "Telefone");
        }
    }
}
