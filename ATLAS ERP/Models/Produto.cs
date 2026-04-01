using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ATLAS_ERP.Models
{
    public class Produto
    {
        public int ProdutoId { get; set; }

        [Required]
        public string Nome { get; set; }

        public string Categoria { get; set; }

        [Required]
        public decimal PrecoVenda { get; set; }

        public int EstoqueMinimo { get; set; }
        public bool Ativo { get; set; }

        [ForeignKey("Empresa")]
        public int EmpresaId { get; set; }
        public Empresa Empresa { get; set; }
    }
}