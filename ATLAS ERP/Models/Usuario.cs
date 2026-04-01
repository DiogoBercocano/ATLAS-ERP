using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ATLAS_ERP.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }


        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string SenhaHash { get; set; }
        public string Role { get; set; }
        public bool Ativo { get; set; }


        [ForeignKey("Empresa")]
        public int? EmpresaId { get; set; }
        public Empresa Empresa { get; set; }
    }
}