using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaAguas.WPF.Models
{
    public class Contador
    {
        public int Id { get; set; }

        public string NumeroContador { get; set; }

        public DateTime DataInstalacao { get; set; }

        public bool Ativo { get; set; }

        public int ClienteId { get; set; }

        public override string ToString()
        {
            return NumeroContador;
        }
    }
}
