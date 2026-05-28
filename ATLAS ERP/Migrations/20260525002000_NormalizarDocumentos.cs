namespace ATLAS_ERP.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class NormalizarDocumentos : DbMigration
    {
        public override void Up()
        {
            // Remove pontuação de CPF/CNPJ legados que possam ter sido gravados com formatação.
            // Documentos já normalizados (só dígitos) não são afetados.
            Sql(@"
                UPDATE dbo.Clientes
                SET Documento = REPLACE(REPLACE(REPLACE(REPLACE(Documento, '.', ''), '-', ''), '/', ''), ' ', '')
                WHERE Documento IS NOT NULL
                  AND Documento LIKE '%[^0-9]%'
            ");

            Sql(@"
                UPDATE dbo.Fornecedors
                SET CNPJ = REPLACE(REPLACE(REPLACE(REPLACE(CNPJ, '.', ''), '-', ''), '/', ''), ' ', '')
                WHERE CNPJ IS NOT NULL
                  AND CNPJ LIKE '%[^0-9]%'
            ");

            Sql(@"
                UPDATE dbo.Empresas
                SET CNPJ = REPLACE(REPLACE(REPLACE(REPLACE(CNPJ, '.', ''), '-', ''), '/', ''), ' ', '')
                WHERE CNPJ IS NOT NULL
                  AND CNPJ LIKE '%[^0-9]%'
            ");
        }

        public override void Down()
        {
            // Não há como reverter normalização de dados sem backup dos valores originais.
        }
    }
}
