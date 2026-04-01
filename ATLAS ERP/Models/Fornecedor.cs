using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ATLAS_ERP.Models
{
    public class Fornecedor
    {
        public int FornecedorId { get; set; }

        [Required]
        public string Nome { get; set; }

        public string CNPJ { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }

        [ForeignKey("Empresa")]
        public int EmpresaId { get; set; }
        public Empresa Empresa { get; set; }
    }
}