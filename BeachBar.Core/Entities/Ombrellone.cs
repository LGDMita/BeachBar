using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BeachBar.Core.Entities
{
    public class Ombrellone
    {
        public int Id { get; set; }
        public int Numero { get; set; }
        public bool Occupato { get; set; }
        public int? CellaIndice { get; set; }
        public ICollection<Sessione> Sessioni { get; set; } = new List<Sessione>();

        /// <summary>
        /// Nome del cliente per ombrelloni "senza lista" (Occupato=true, nessuna sessione aperta).
        /// Popolato in memoria da GetOmbrelloniAsync, non salvato nel DB.
        /// </summary>
        [NotMapped]
        public string? NomeClienteAttivo { get; set; }
    }
}
