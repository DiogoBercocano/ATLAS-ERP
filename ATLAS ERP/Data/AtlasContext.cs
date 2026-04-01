using ATLAS_ERP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ATLAS_ERP.Data
{
    public class AtlasContext :DbContext
    {
        public AtlasContext() : base("DefaultConnection")
        {
        }

        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Filial> Filiais { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<Venda> Vendas { get; set; }
        public DbSet<VendaItem> VendaItens { get; set; }
        public DbSet<ContaReceber> ContasReceber { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.OneToManyCascadeDeleteConvention>();
        }
    }
}