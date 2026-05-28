using System;
using System.Collections.Generic;
using System.Text;

namespace BeachBar.Core.Entities
{
    public class Consumazione
    {
        public int Id { get; set; }
        public int SessioneId { get; set; }
        public Sessione Sessione { get; set; } = null!;
        public int ProdottoId { get; set; }
        public Prodotto Prodotto { get; set; } = null!;
        public int Quantita { get; set; }
        public DateTime Timestamp { get; set; }
        /// <summary>Giorno in cui il prodotto è stato ordinato (per visualizzazione multi-giorno).</summary>
        public DateOnly Giorno { get; set; }
    }
}
