using System;
using System.Collections.Generic;
using System.Text;

namespace BeachBar.Core.Entities
{
    // Una sessione rappresenta un "conto aperto" su un ombrellone per un periodo.
    // OmbrelloneId == null indica un conto volante (ospite senza postazione fissa).
    public class Sessione
    {
        public int Id { get; set; }
        public int? OmbrelloneId { get; set; }
        public Ombrellone? Ombrellone { get; set; }
        public string? NomeCliente { get; set; }
        public DateTime Apertura { get; set; }
        public DateTime? Chiusura { get; set; }
        public bool Chiusa { get; set; }

        // DataRiferimento = giorno d'inizio. È il campo usato nei filtri per data.
        // Per soggiorni multi-giorno la sessione è "attiva" per qualsiasi data in [DataRiferimento, DataFine].
        public DateOnly? DataRiferimento { get; set; }

        /// <summary>Data di fine soggiorno per prenotazioni multi-giorno. Null = giorno singolo.</summary>
        public DateOnly? DataFine { get; set; }
        public ICollection<Consumazione> Consumazioni { get; set; } = new List<Consumazione>();

    }
}
