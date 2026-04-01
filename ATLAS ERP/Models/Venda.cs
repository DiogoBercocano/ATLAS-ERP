using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ATLAS_ERP.Models
{
    public class Venda
    {
        public int VendaId { get; set; }

        public DateTime DataVenda { get; set; }

        public decimal Total { get; set; }

        public string Status { get; set; }

        [ForeignKey("Empresa")]
        public int EmpresaId { get; set; }
        public Empresa Empresa { get; set; }

        [ForeignKey("Cliente")]
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        [ForeignKey("Usuario")]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        public ICollection<VendaItem> Itens { get; set; }
        public ICollection<ContaReceber> ContasReceber { get; set; }
    }
}