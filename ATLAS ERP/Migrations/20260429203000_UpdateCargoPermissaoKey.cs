namespace ATLAS_ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class UpdateCargoPermissaoKey : DbMigration
    {
        public override void Up()
        {
            // A tabela CargoPermissoes já foi criada na migration anterior
            // Esta migration apenas garante que a chave composta está definida corretamente
            // Nenhuma alteração SQL necessária pois a migration anterior já criou a tabela com a chave correta
        }

        public override void Down()
        {
            // Nenhuma ação necessária no rollback
        }
    }
}
