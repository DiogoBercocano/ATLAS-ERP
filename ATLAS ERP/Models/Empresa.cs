using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static System.Data.Entity.Migrations.Model.UpdateDatabaseOperation;

namespace ATLAS_ERP.Models
{
    public class Empresa
    {
        public int EmpresaId { get; set; }

        [Required]
        [StringLength(150)]
        public string Nome { get; set; }

        [Required]
        [StringLength(14)]
        public string CNPJ { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public bool Ativa { get; set; }

        public string LogoPath { get; set; }

        // "Pendente" ou "Ativa"
        public string Status { get; set; }

        public ICollection<Filial> Filials { get; set; }
        public ICollection<Usuario> Ususarios { get; set; }
        public ICollection<Cliente> Clientes { get; set; }
        public ICollection<Produto> Produtos { get; set; }
        public ICollection<Fornecedor> Fornecedores { get; set; }
        public ICollection<Venda> Vendas { get; set; }
    }
}