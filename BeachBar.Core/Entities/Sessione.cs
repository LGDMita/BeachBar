using System;
using System.Collections.Generic;
using System.Text;

namespace BeachBar.Core.Entities
{
    public class Sessione
    {
        public int Id { get; set; }
        public int? OmbrelloneId { get; set; }
        public Ombrellone? Ombrellone { get; set; }
        public string? NomeCliente { get; set; }
        public DateTime Apertura { get; set; }
        public DateTime? Chiusura { get; set; }
        public bool Chiusa { get; set; }
        public DateOnly? DataRiferimento { get; set; }
        public ICollection<Consumazione> Consumazioni { get; set; } = new List<Consumazione>();

    }
}
