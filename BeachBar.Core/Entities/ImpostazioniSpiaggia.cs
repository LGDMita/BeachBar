using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace BeachBar.Core.Entities;

// Singleton di configurazione (Id sempre = 1). Non è una tabella di lookup generica:
// contiene sia i parametri di layout sia il cursore per il reset visivo delle statistiche.
public class ImpostazioniSpiaggia
{
    public int Id { get; set; }
    public int NumeroOmbrelloni { get; set; }
    public int NumeroColonne { get; set; }
    public int NumeroRighe { get; set; } = 5;

    // Separatori visuali nella griglia. Stringa CSV di indici 0-based (es. "2,5").
    // BordiVerticali  = colonne dopo cui tracciare una linea verticale.
    // BordiOrizzontali = righe dopo cui tracciare una linea orizzontale.
    // Null/vuoto = nessun bordo. CSV scelto per semplicità: EF Core non gestisce
    // liste di primitivi su PostgreSQL senza conversioni custom o colonne JSON.
    public string? BordiVerticali { get; set; }
    public string? BordiOrizzontali { get; set; }

    // Soglia per il calcolo "incassato oggi": esclude le sessioni chiuse prima di questo timestamp.
    // Usato per azzerare il contatore visivo senza perdere i dati storici.
    public DateTime? UltimoResetStatistiche { get; set; }

    // ── Computed (non mappati su DB) ──────────────────────────────────────

    /// <summary>Indici delle colonne separatore, già parsati dal CSV.</summary>
    [NotMapped]
    public HashSet<int> BordiVerticaliSet => ParseCsvInts(BordiVerticali);

    /// <summary>Indici delle righe separatore, già parsati dal CSV.</summary>
    [NotMapped]
    public HashSet<int> BordiOrizzontaliSet => ParseCsvInts(BordiOrizzontali);

    // L'entità possiede il parsing dei propri campi CSV: evita duplicazione
    // tra Home.razor e ImpostazioniService.
    private static HashSet<int> ParseCsvInts(string? s) =>
        s == null
            ? new HashSet<int>()
            : s.Split(',', StringSplitOptions.RemoveEmptyEntries)
               .Where(x => int.TryParse(x, out _))
               .Select(int.Parse)
               .ToHashSet();
}
