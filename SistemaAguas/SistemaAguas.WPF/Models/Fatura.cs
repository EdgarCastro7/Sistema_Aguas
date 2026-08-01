using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaAguas.WPF.Models
{
    public class Fatura
    {
        public int Id { get; set; }

        public DateTime DataFatura { get; set; }

        public double ValorTotal { get; set; }

        public bool Pago {  get; set; }

        public bool Anulada { get; set; }

        public int ClienteId { get; set; }

        public int ContadorId { get; set; }

        public int ConsumoId { get; set; }

        public override string ToString()
        {
            return $"Fatura {Id} - {ValorTotal}€";
        }
    }
}
