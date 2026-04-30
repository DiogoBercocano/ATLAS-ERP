using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ATLAS_ERP.Models
{
    public class Permissao
    {
        public int PermissaoId { get; set; }

        [Required]
        [StringLength(100)]
        public string Chave { get; set; }

        [Required]
        [StringLength(200)]
        public string Nome { get; set; }

        [StringLength(500)]
        public string Descricao { get; set; }

        public string Categoria { get; set; }

        public virtual ICollection<CargoPermissao> CargoPermissoes { get; set; }
    }
}
