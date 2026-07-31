using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaAguas.WPF.Models
{
    public class Consumo
    {
        public int Id { get; set; }

        public DateTime DataLeitura { get; set; }

        public double ValorConsumido { get; set; }

        public double LeituraAtual { get; set; }

        public double LeituraAnterior { get; set; }

        public int ContadorId { get; set; }

        public override string ToString()
        {
            return ValorConsumido.ToString();
        }
    }
}
