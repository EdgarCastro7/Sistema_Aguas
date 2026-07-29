using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SistemaAguas.WPF.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        public string Nome { get; set; }

        public string Morada { get; set; }

        public string NIF { get; set; }

        public string Contacto { get; set; }

        public string Email { get; set; }

        public string CodigoPostal { get; set; }

        public DateTime Registo { get; set; }

        public bool Ativo { get; set; }

        public override string ToString()
        {
            return $"{Id} - {Nome}";
        }
    }
}
