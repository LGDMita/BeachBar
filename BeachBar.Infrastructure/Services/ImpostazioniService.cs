using BeachBar.Core.Entities;
using BeachBar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BeachBar.Infrastructure.Services;

public class ImpostazioniService : IImpostazioniService
{
    private readonly BeachBarDbContext _db;

    public ImpostazioniService(BeachBarDbContext db) => _db = db;

    // Helper interno: l'intera configurazione è un singleton (Id=1 garantito dal seed).
    // Tutti i metodi pubblici passano da qui così un eventuale refactoring del lookup è in un unico punto.
    private async Task<ImpostazioniSpiaggia> GetConfigAsync()
    {
        return await _db.ImpostazioniSpiaggia.FirstOrDefaultAsync(i => i.Id == 1)
            ?? throw new InvalidOperationException(
                "Configurazione spiaggia non trovata (Id=1). Verificare il seed del database.");
    }

    public async Task<ImpostazioniSpiaggia> GetImpostazioniAsync()
        => await GetConfigAsync();

    public async Task AggiornaColumeAsync(int colonne)
    {
        var imp = await GetConfigAsync();
        imp.NumeroColonne = colonne;
        await _db.SaveChangesAsync();
    }

    public async Task AggiornaOmbrelloniAsync(int numero)
    {
        var imp = await GetConfigAsync();
        var attuali = await _db.Ombrelloni.CountAsync();

        if (numero > attuali)
        {
            for (int i = attuali + 1; i <= numero; i++)
                _db.Ombrelloni.Add(new Ombrellone { Numero = i, Occupato = false });
        }
        else if (numero < attuali)
        {
            // Rimuove solo gli ombrelloni liberi con numero > soglia: quelli occupati
            // vengono preservati per non perdere sessioni aperte.
            var daRimuovere = await _db.Ombrelloni
                .Where(o => o.Numero > numero && !o.Occupato)
                .ToListAsync();
            _db.Ombrelloni.RemoveRange(daRimuovere);
        }

        imp.NumeroOmbrelloni = numero;
        await _db.SaveChangesAsync();
    }

    public async Task<(decimal aperto, decimal incassato, int ombrelloniAttivi)> GetStatisticheAsync(DateOnly data)
    {
        var aperto = await _db.Sessioni
            .Where(s => !s.Chiusa && s.DataRiferimento <= data && (s.DataFine == null || s.DataFine >= data))
            .SelectMany(s => s.Consumazioni)
            .SumAsync(c => (decimal?)c.Quantita * c.Prodotto.Prezzo) ?? 0;

        var oggi = DateOnly.FromDateTime(DateTime.Today);
        IQueryable<Sessione> chiuseQuery = _db.Sessioni.Where(s => s.Chiusa && s.DataRiferimento == data);
        if (data == oggi)
        {
            // Il reset visivo non cancella i dati: filtra per Chiusura >= soglia così
            // le sessioni chiuse prima del reset non rientrano nel contatore "incassato oggi".
            var imp = await GetConfigAsync();
            if (imp.UltimoResetStatistiche.HasValue)
                chiuseQuery = chiuseQuery.Where(s => s.Chiusura >= imp.UltimoResetStatistiche.Value);
        }

        var incassato = await chiuseQuery
            .SelectMany(s => s.Consumazioni)
            .SumAsync(c => (decimal?)c.Quantita * c.Prodotto.Prezzo) ?? 0;

        int attivi;
        if (data == oggi)
        {
            // Per oggi il flag Occupato è la fonte di verità (include "senza lista")
            attivi = await _db.Ombrelloni.CountAsync(o => o.Occupato);
        }
        else
        {
            // Per date diverse da oggi il flag Occupato riflette solo la situazione attuale,
            // quindi si conta dalle sessioni aperte in quella data.
            attivi = await _db.Sessioni
                .CountAsync(s => !s.Chiusa && s.OmbrelloneId != null
                              && s.DataRiferimento <= data
                              && (s.DataFine == null || s.DataFine >= data));
        }

        return (aperto, incassato, attivi);
    }

    public async Task ResetVisivoStatisticheAsync()
    {
        var imp = await GetConfigAsync();
        imp.UltimoResetStatistiche = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ── Layout personalizzato ──────────────────────────────────────────────

    public async Task AggiornaGrigliaAsync(int righe, int colonne)
    {
        var imp = await GetConfigAsync();
        bool cambiate = imp.NumeroRighe != righe || imp.NumeroColonne != colonne;

        if (cambiate)
        {
            // Cambiando le dimensioni le celle cambiano indice: resettare tutto è più sicuro
            // che provare a ricalcolare le posizioni (potrebbe collocare ombrelloni fuori griglia).
            var tutti = await _db.Ombrelloni.ToListAsync();
            foreach (var o in tutti) o.CellaIndice = null;

            // I bordi sono indici 0-based relativi alla griglia corrente: se la nuova griglia
            // è più piccola, i bordi oltre i nuovi limiti diventano invalidi e vanno rimossi.
            var v = imp.BordiVerticaliSet.Where(c => c < colonne - 1).ToList();
            var h = imp.BordiOrizzontaliSet.Where(r => r < righe - 1).ToList();
            imp.BordiVerticali = v.Count > 0 ? string.Join(",", v) : null;
            imp.BordiOrizzontali = h.Count > 0 ? string.Join(",", h) : null;
        }

        imp.NumeroRighe = righe;
        imp.NumeroColonne = colonne;
        await _db.SaveChangesAsync();
    }

    public async Task AggiornaBordiAsync(IEnumerable<int> bordiV, IEnumerable<int> bordiH)
    {
        var imp = await GetConfigAsync();
        var v = bordiV.OrderBy(x => x).ToList();
        var h = bordiH.OrderBy(x => x).ToList();
        imp.BordiVerticali = v.Count > 0 ? string.Join(",", v) : null;
        imp.BordiOrizzontali = h.Count > 0 ? string.Join(",", h) : null;
        await _db.SaveChangesAsync();
    }

    public async Task<List<Ombrellone>> GetOmbrelloniPerEditorAsync()
        => await _db.Ombrelloni.AsNoTracking().OrderBy(o => o.Numero).ToListAsync();

    public async Task AssegnaCellaAsync(int cellaIndice)
    {
        // Controllo server-side: la cella potrebbe essere già occupata se due tab
        // aprono l'editor contemporaneamente (Blazor Server non garantisce atomicità).
        if (await _db.Ombrelloni.AnyAsync(o => o.CellaIndice == cellaIndice))
            return;

        var omb = await _db.Ombrelloni
            .Where(o => o.CellaIndice == null)
            .OrderBy(o => o.Numero)
            .FirstOrDefaultAsync();

        if (omb == null)
        {
            var maxNum = await _db.Ombrelloni.MaxAsync(o => (int?)o.Numero) ?? 0;
            omb = new Ombrellone { Numero = maxNum + 1, Occupato = false };
            _db.Ombrelloni.Add(omb);
        }

        omb.CellaIndice = cellaIndice;
        await _db.SaveChangesAsync();
    }

    public async Task AssegnaCelleAsync(IReadOnlyList<int> celleIndici)
    {
        if (celleIndici.Count == 0) return;

        var occupate = (await _db.Ombrelloni
            .Where(o => o.CellaIndice != null)
            .Select(o => o.CellaIndice!.Value)
            .ToListAsync())
            .ToHashSet();

        var daPosizionare = celleIndici
            .Distinct()
            .Where(idx => !occupate.Contains(idx))
            .ToList();

        if (daPosizionare.Count == 0) return;

        var nonPosizionati = await _db.Ombrelloni
            .Where(o => o.CellaIndice == null)
            .OrderBy(o => o.Numero)
            .ToListAsync();

        var maxNum = await _db.Ombrelloni.MaxAsync(o => (int?)o.Numero) ?? 0;

        for (int i = 0; i < daPosizionare.Count; i++)
        {
            Ombrellone omb;
            if (i < nonPosizionati.Count)
            {
                omb = nonPosizionati[i];
            }
            else
            {
                omb = new Ombrellone { Numero = ++maxNum, Occupato = false };
                _db.Ombrelloni.Add(omb);
            }
            omb.CellaIndice = daPosizionare[i];
        }

        await _db.SaveChangesAsync();
    }

    public async Task RimuoviDaCellaAsync(int cellaIndice)
    {
        var omb = await _db.Ombrelloni.FirstOrDefaultAsync(o => o.CellaIndice == cellaIndice);
        if (omb != null)
        {
            omb.CellaIndice = null;
            await _db.SaveChangesAsync();
        }
    }

    public async Task PopolaSequenzialeAsync()
    {
        var imp = await GetConfigAsync();
        int totCelle = imp.NumeroRighe * imp.NumeroColonne;

        var ombrelloni = await _db.Ombrelloni.OrderBy(o => o.Numero).ToListAsync();

        // Azzera tutte le posizioni correnti
        foreach (var o in ombrelloni) o.CellaIndice = null;

        // Crea gli ombrelloni mancanti per riempire tutta la griglia
        int maxNum = ombrelloni.Count > 0 ? ombrelloni.Max(o => o.Numero) : 0;
        while (ombrelloni.Count < totCelle)
        {
            var nuovo = new Ombrellone { Numero = ++maxNum, Occupato = false };
            _db.Ombrelloni.Add(nuovo);
            ombrelloni.Add(nuovo);
        }

        // Assegna posizioni sequenziali
        for (int i = 0; i < totCelle; i++)
            ombrelloni[i].CellaIndice = i;

        imp.NumeroOmbrelloni = totCelle;
        await _db.SaveChangesAsync();
    }

    public async Task AzzeraLayoutAsync()
    {
        var ombrelloni = await _db.Ombrelloni.ToListAsync();
        foreach (var o in ombrelloni) o.CellaIndice = null;
        await _db.SaveChangesAsync();
    }
}
