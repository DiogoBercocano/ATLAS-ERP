using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ATLAS_ERP.Models
{
    public class ContaReceber
    {
        public int ContaReceberId { get; set; }

        public DateTime DataVencimento { get; set; }
        public decimal Valor { get; set; }
        public bool Pago { get; set; }
        public DateTime? DataPagamento { get; set; }

        [ForeignKey("Venda")]
        public int VendaId { get; set; }
        public Venda Venda { get; set; }
    }
}