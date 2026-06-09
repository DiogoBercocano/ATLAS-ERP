namespace ATLAS_ERP.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddPerformanceIndexes : DbMigration
    {
        public override void Up()
        {
            // Compostos (EmpresaId, coluna de busca/ordenação) — cobrem os Index das páginas paginadas
            Sql("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Produto_Empresa_Nome' AND object_id = OBJECT_ID('dbo.Produtoes')) CREATE INDEX IX_Produto_Empresa_Nome ON dbo.Produtoes(EmpresaId, Nome)");
            Sql("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Produto_Empresa_Categoria' AND object_id = OBJECT_ID('dbo.Produtoes')) CREATE INDEX IX_Produto_Empresa_Categoria ON dbo.Produtoes(EmpresaId, Categoria)");
            Sql("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Cliente_Empresa_Nome' AND object_id = OBJECT_ID('dbo.Clientes')) CREATE INDEX IX_Cliente_Empresa_Nome ON dbo.Clientes(EmpresaId, Nome)");
            Sql("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Fornecedor_Empresa_Nome' AND object_id = OBJECT_ID('dbo.Fornecedors')) CREATE INDEX IX_Fornecedor_Empresa_Nome ON dbo.Fornecedors(EmpresaId, Nome)");
            Sql("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Usuario_Empresa_Name' AND object_id = OBJECT_ID('dbo.Usuarios')) CREATE INDEX IX_Usuario_Empresa_Name ON dbo.Usuarios(EmpresaId, Name)");

            // DataVenda DESC — cobre o ORDER BY padrão no Index de vendas
            Sql("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Venda_Empresa_DataDesc' AND object_id = OBJECT_ID('dbo.Vendas')) CREATE INDEX IX_Venda_Empresa_DataDesc ON dbo.Vendas(EmpresaId, DataVenda DESC)");

            // Email do usuário — cobre o login lookup (WHERE Email = ?)
            Sql("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Usuario_Email' AND object_id = OBJECT_ID('dbo.Usuarios')) CREATE INDEX IX_Usuario_Email ON dbo.Usuarios(Email)");

            // Pago — cobre queries financeiras (WHERE Pago = 0)
            Sql("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ContaReceber_Pago' AND object_id = OBJECT_ID('dbo.ContaRecebers')) CREATE INDEX IX_ContaReceber_Pago ON dbo.ContaRecebers(Pago)");
        }

        public override void Down()
        {
            Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Produto_Empresa_Nome' AND object_id = OBJECT_ID('dbo.Produtoes')) DROP INDEX IX_Produto_Empresa_Nome ON dbo.Produtoes");
            Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Produto_Empresa_Categoria' AND object_id = OBJECT_ID('dbo.Produtoes')) DROP INDEX IX_Produto_Empresa_Categoria ON dbo.Produtoes");
            Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Cliente_Empresa_Nome' AND object_id = OBJECT_ID('dbo.Clientes')) DROP INDEX IX_Cliente_Empresa_Nome ON dbo.Clientes");
            Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Fornecedor_Empresa_Nome' AND object_id = OBJECT_ID('dbo.Fornecedors')) DROP INDEX IX_Fornecedor_Empresa_Nome ON dbo.Fornecedors");
            Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Usuario_Empresa_Name' AND object_id = OBJECT_ID('dbo.Usuarios')) DROP INDEX IX_Usuario_Empresa_Name ON dbo.Usuarios");
            Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Venda_Empresa_DataDesc' AND object_id = OBJECT_ID('dbo.Vendas')) DROP INDEX IX_Venda_Empresa_DataDesc ON dbo.Vendas");
            Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Usuario_Email' AND object_id = OBJECT_ID('dbo.Usuarios')) DROP INDEX IX_Usuario_Email ON dbo.Usuarios");
            Sql("IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ContaReceber_Pago' AND object_id = OBJECT_ID('dbo.ContaRecebers')) DROP INDEX IX_ContaReceber_Pago ON dbo.ContaRecebers");
        }
    }
}
