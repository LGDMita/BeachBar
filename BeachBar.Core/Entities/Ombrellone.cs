using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BeachBar.Core.Entities
{
    // Rappresenta un posto fisico sulla spiaggia. Lo stato "occupato" è persisted nel DB
    // e sopravvive alla chiusura delle singole sessioni giornaliere.
    public class Ombrellone
    {
        public int Id { get; set; }
        public int Numero { get; set; }

        // Flag esplicito separato dalle sessioni: un ombrellone può essere Occupato=true
        // anche senza sessioni aperte (cliente fisso che ha pagato ma non ha ancora una lista).
        // Impostato a false solo da ChiudiSessione(liberaOmbrellone:true) o LiberaOmbrelloneAsync.
        public bool Occupato { get; set; }

        // Indice di posizione nella griglia (riga*numColonne+colonna). Null = non posizionato.
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
