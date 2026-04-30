using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ATLAS_ERP.Models
{
    public class CargoPermissao
    {
        [Key]
        [Column(Order = 0)]
        [ForeignKey("Cargo")]
        public int CargoId { get; set; }
        public virtual Cargo Cargo { get; set; }

        [Key]
        [Column(Order = 1)]
        [ForeignKey("Permissao")]
        public int PermissaoId { get; set; }
        public virtual Permissao Permissao { get; set; }
    }
}
