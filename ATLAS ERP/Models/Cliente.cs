using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Data.Entity.Migrations.Model.UpdateDatabaseOperation;

namespace ATLAS_ERP.Models
{
    public class Cliente
    {
        public int ClienteId { get; set; }

        [Required]
        public string Nome { get; set; }

        public string Documento { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string Telefone { get; set; }

        public string Endereco { get; set; }

        public decimal LimiteCredito { get; set; }

        public bool Ativo { get; set; }

        [ForeignKey("Empresa")]
        public int EmpresaId { get; set; }
        public Empresa Empresa { get; set; }
    }
}