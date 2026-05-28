using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ATLAS_ERP.Models
{
    [Table("Cargos")]
    public class Cargo
    {
        public int CargoId { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [StringLength(500)]
        public string Descricao { get; set; }

        public bool Ativo { get; set; } = true;

        [ForeignKey("Empresa")]
        public int EmpresaId { get; set; }
        public Empresa Empresa { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public virtual ICollection<Usuario> Usuarios { get; set; }
        public virtual ICollection<CargoPermissao> CargoPermissoes { get; set; }
    }
}
