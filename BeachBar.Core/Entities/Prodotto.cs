using System;
using System.Collections.Generic;
using System.Text;

namespace BeachBar.Core.Entities
{
    public class Prodotto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Prezzo { get; set; }
        public bool Disponibile { get; set; } = true;
        public string Categoria { get; set; } = string.Empty;
        public ICollection<Consumazione> Consumazioni { get; set; } = new List<Consumazione>();

    }
}
