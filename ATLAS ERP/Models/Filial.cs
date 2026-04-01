using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ATLAS_ERP.Models
{
    public class Filial
    {
        public int FilialId { get; set; }

        [Required]
        public string Nome { get; set; }

        public string Cidadde { get; set; }
        public string Estado { get; set; }

        [ForeignKey("Empresa")]
        public int EmpresaId { get; set; }
        public Empresa Empresa { get; set; }
    }
}