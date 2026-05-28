using BeachBar.Core.Entities;

namespace BeachBar.Infrastructure.Services;

public interface IImpostazioniService
{
    Task<ImpostazioniSpiaggia> GetImpostazioniAsync();
    Task AggiornaColumeAsync(int colonne);

    /// <summary>Aggiunge o rimuove ombrelloni per raggiungere il numero desiderato.</summary>
    Task AggiornaOmbrelloniAsync(int numero);

    /// <summary>Calcola il totale aperto, l'incassato e gli ombrelloni attivi per la data specificata.</summary>
    Task<(decimal aperto, decimal incassato, int ombrelloniAttivi)> GetStatisticheAsync(DateOnly data);

    /// <summary>Aggiorna la data di reset visivo per azzerare i contatori della dashboard.</summary>
    Task ResetVisivoStatisticheAsync();

    // ── Layout personalizzato ──────────────────────────────────────────────

    /// <summary>Cambia dimensioni griglia. Se cambiano, azzera tutte le posizioni (CellaIndice).</summary>
    Task AggiornaGrigliaAsync(int righe, int colonne);

    /// <summary>Salva i bordi verticali (dopo colonna c) e orizzontali (dopo riga r).</summary>
    Task AggiornaBordiAsync(IEnumerable<int> bordiV, IEnumerable<int> bordiH);

    /// <summary>Restituisce tutti gli ombrelloni ordinati per Numero (include CellaIndice).</summary>
    Task<List<Ombrellone>> GetOmbrelloniPerEditorAsync();

    /// <summary>Assegna il prossimo ombrellone non posizionato alla cella; se non ne esistono, ne crea uno nuovo.</summary>
    Task AssegnaCellaAsync(int cellaIndice);

    /// <summary>Assegna sequenzialmente più celle vuote in un'unica transazione. Usato per il drag nella mappa.</summary>
    Task AssegnaCelleAsync(IReadOnlyList<int> celleIndici);

    /// <summary>Rimuove l'ombrellone dalla cella (CellaIndice → null).</summary>
    Task RimuoviDaCellaAsync(int cellaIndice);

    /// <summary>Assegna posizioni sequenziali (sin→destra, riga per riga) a tutti gli ombrelloni non posizionati.</summary>
    Task PopolaSequenzialeAsync();

    /// <summary>Azzera CellaIndice di tutti gli ombrelloni.</summary>
    Task AzzeraLayoutAsync();
}
