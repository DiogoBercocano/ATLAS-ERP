namespace ATLAS_ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddDefaultPermissoes : DbMigration
    {
        public override void Up()
        {
            Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Permissaos WHERE Chave = 'dashboard_view')
                BEGIN
                    INSERT INTO Permissaos (Chave, Nome, Categoria) VALUES ('dashboard_view', 'Visualizar Dashboard', 'Dashboard')
                END

                IF NOT EXISTS (SELECT 1 FROM Permissaos WHERE Chave = 'vendas_view')
                BEGIN
                    INSERT INTO Permissaos (Chave, Nome, Categoria) VALUES
                    ('vendas_view', 'Visualizar Vendas', 'Vendas'),
                    ('vendas_create', 'Criar Vendas', 'Vendas'),
                    ('vendas_edit', 'Editar Vendas', 'Vendas'),
                    ('vendas_delete', 'Deletar Vendas', 'Vendas'),
                    ('produtos_view', 'Visualizar Produtos', 'Produtos'),
                    ('produtos_create', 'Criar Produtos', 'Produtos'),
                    ('produtos_edit', 'Editar Produtos', 'Produtos'),
                    ('produtos_delete', 'Deletar Produtos', 'Produtos'),
                    ('clientes_view', 'Visualizar Clientes', 'Clientes'),
                    ('clientes_create', 'Criar Clientes', 'Clientes'),
                    ('clientes_edit', 'Editar Clientes', 'Clientes'),
                    ('clientes_delete', 'Deletar Clientes', 'Clientes'),
                    ('fornecedores_view', 'Visualizar Fornecedores', 'Fornecedores'),
                    ('fornecedores_create', 'Criar Fornecedores', 'Fornecedores'),
                    ('fornecedores_edit', 'Editar Fornecedores', 'Fornecedores'),
                    ('fornecedores_delete', 'Deletar Fornecedores', 'Fornecedores'),
                    ('usuarios_view', 'Visualizar Funcionários', 'Funcionários'),
                    ('usuarios_create', 'Criar Funcionários', 'Funcionários'),
                    ('usuarios_edit', 'Editar Funcionários', 'Funcionários'),
                    ('usuarios_delete', 'Deletar Funcionários', 'Funcionários'),
                    ('cargos_manage', 'Gerenciar Cargos', 'Administração'),
                    ('settings', 'Configurações', 'Administração')
                END
            ");

            Sql(@"
                DECLARE @AdminCargoId INT
                SELECT @AdminCargoId = CargoId FROM Cargos WHERE Nome = 'Admin'

                IF @AdminCargoId IS NOT NULL
                BEGIN
                    INSERT INTO CargoPermissaos (CargoId, PermissaoId)
                    SELECT @AdminCargoId, PermissaoId FROM Permissaos
                    WHERE NOT EXISTS (
                        SELECT 1 FROM CargoPermissaos WHERE CargoId = @AdminCargoId AND PermissaoId = Permissaos.PermissaoId
                    )
                END
            ");
        }

        public override void Down()
        {
            DropTable("dbo.Permissaos");
        }
    }
}
