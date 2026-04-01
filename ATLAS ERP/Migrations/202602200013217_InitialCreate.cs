namespace ATLAS_ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Clientes",
                c => new
                    {
                        ClienteId = c.Int(nullable: false, identity: true),
                        Nome = c.String(nullable: false),
                        Documento = c.String(),
                        Email = c.String(),
                        LimiteCredito = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Ativo = c.Boolean(nullable: false),
                        EmpresaId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ClienteId)
                .ForeignKey("dbo.Empresas", t => t.EmpresaId)
                .Index(t => t.EmpresaId);
            
            CreateTable(
                "dbo.Empresas",
                c => new
                    {
                        EmpresaId = c.Int(nullable: false, identity: true),
                        Nome = c.String(nullable: false, maxLength: 150),
                        CNPJ = c.String(nullable: false, maxLength: 14),
                        Email = c.String(),
                        Ativa = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.EmpresaId);
            
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
                .PrimaryKey(t => t.FilialId)
                .ForeignKey("dbo.Empresas", t => t.EmpresaId)
                .Index(t => t.EmpresaId);
            
            CreateTable(
                "dbo.Fornecedors",
                c => new
                    {
                        FornecedorId = c.Int(nullable: false, identity: true),
                        Nome = c.String(nullable: false),
                        CNPJ = c.String(),
                        Telefone = c.String(),
                        Email = c.String(),
                        EmpresaId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.FornecedorId)
                .ForeignKey("dbo.Empresas", t => t.EmpresaId)
                .Index(t => t.EmpresaId);
            
            CreateTable(
                "dbo.Produtoes",
                c => new
                    {
                        ProdutoId = c.Int(nullable: false, identity: true),
                        Nome = c.String(nullable: false),
                        Categoria = c.String(),
                        PrecoVenda = c.Decimal(nullable: false, precision: 18, scale: 2),
                        EstoqueMinimo = c.Int(nullable: false),
                        Ativo = c.Boolean(nullable: false),
                        EmpresaId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ProdutoId)
                .ForeignKey("dbo.Empresas", t => t.EmpresaId)
                .Index(t => t.EmpresaId);
            
            CreateTable(
                "dbo.Usuarios",
                c => new
                    {
                        UsuarioId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        SenhaHash = c.String(),
                        Role = c.String(),
                        Ativo = c.Boolean(nullable: false),
                        EmpresaId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UsuarioId)
                .ForeignKey("dbo.Empresas", t => t.EmpresaId)
                .Index(t => t.EmpresaId);
            
            CreateTable(
                "dbo.Vendas",
                c => new
                    {
                        VendaId = c.Int(nullable: false, identity: true),
                        DataVenda = c.DateTime(nullable: false),
                        Total = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.String(),
                        EmpresaId = c.Int(nullable: false),
                        ClienteId = c.Int(nullable: false),
                        UsuarioId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.VendaId)
                .ForeignKey("dbo.Clientes", t => t.ClienteId)
                .ForeignKey("dbo.Empresas", t => t.EmpresaId)
                .ForeignKey("dbo.Usuarios", t => t.UsuarioId)
                .Index(t => t.EmpresaId)
                .Index(t => t.ClienteId)
                .Index(t => t.UsuarioId);
            
            CreateTable(
                "dbo.ContaRecebers",
                c => new
                    {
                        ContaReceberId = c.Int(nullable: false, identity: true),
                        DataVencimento = c.DateTime(nullable: false),
                        Valor = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Pago = c.Boolean(nullable: false),
                        DataPagamento = c.DateTime(),
                        VendaId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ContaReceberId)
                .ForeignKey("dbo.Vendas", t => t.VendaId)
                .Index(t => t.VendaId);
            
            CreateTable(
                "dbo.VendaItems",
                c => new
                    {
                        VendaItemId = c.Int(nullable: false, identity: true),
                        Quantidade = c.Int(nullable: false),
                        PrecoUnitario = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Subtotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                        VendaId = c.Int(nullable: false),
                        ProdutoId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.VendaItemId)
                .ForeignKey("dbo.Produtoes", t => t.ProdutoId)
                .ForeignKey("dbo.Vendas", t => t.VendaId)
                .Index(t => t.VendaId)
                .Index(t => t.ProdutoId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Clientes", "EmpresaId", "dbo.Empresas");
            DropForeignKey("dbo.Vendas", "UsuarioId", "dbo.Usuarios");
            DropForeignKey("dbo.VendaItems", "VendaId", "dbo.Vendas");
            DropForeignKey("dbo.VendaItems", "ProdutoId", "dbo.Produtoes");
            DropForeignKey("dbo.Vendas", "EmpresaId", "dbo.Empresas");
            DropForeignKey("dbo.ContaRecebers", "VendaId", "dbo.Vendas");
            DropForeignKey("dbo.Vendas", "ClienteId", "dbo.Clientes");
            DropForeignKey("dbo.Usuarios", "EmpresaId", "dbo.Empresas");
            DropForeignKey("dbo.Produtoes", "EmpresaId", "dbo.Empresas");
            DropForeignKey("dbo.Fornecedors", "EmpresaId", "dbo.Empresas");
            DropForeignKey("dbo.Filials", "EmpresaId", "dbo.Empresas");
            DropIndex("dbo.VendaItems", new[] { "ProdutoId" });
            DropIndex("dbo.VendaItems", new[] { "VendaId" });
            DropIndex("dbo.ContaRecebers", new[] { "VendaId" });
            DropIndex("dbo.Vendas", new[] { "UsuarioId" });
            DropIndex("dbo.Vendas", new[] { "ClienteId" });
            DropIndex("dbo.Vendas", new[] { "EmpresaId" });
            DropIndex("dbo.Usuarios", new[] { "EmpresaId" });
            DropIndex("dbo.Produtoes", new[] { "EmpresaId" });
            DropIndex("dbo.Fornecedors", new[] { "EmpresaId" });
            DropIndex("dbo.Filials", new[] { "EmpresaId" });
            DropIndex("dbo.Clientes", new[] { "EmpresaId" });
            DropTable("dbo.VendaItems");
            DropTable("dbo.ContaRecebers");
            DropTable("dbo.Vendas");
            DropTable("dbo.Usuarios");
            DropTable("dbo.Produtoes");
            DropTable("dbo.Fornecedors");
            DropTable("dbo.Filials");
            DropTable("dbo.Empresas");
            DropTable("dbo.Clientes");
        }
    }
}
