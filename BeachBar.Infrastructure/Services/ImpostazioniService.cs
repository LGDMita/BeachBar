using BeachBar.Core.Entities;
using BeachBar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BeachBar.Infrastructure.Services;

public class ImpostazioniService : IImpostazioniService
{
    private readonly BeachBarDbContext _db;

    public ImpostazioniService(BeachBarDbContext db) => _db = db;

    // FirstAsync lancerebbe InvalidOperationException con messaggio generico se il record manca.
    // Questo helper usa FirstOrDefaultAsync e produce un errore leggibile che indica il problema reale.
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
            .Where(s => !s.Chiusa && s.DataRiferimento == data)
            .SelectMany(s => s.Consumazioni)
            .SumAsync(c => (decimal?)c.Quantita * c.Prodotto.Prezzo) ?? 0;

        // Per il giorno corrente si applica il filtro di reset visivo, per gli altri giorni si mostra il totale completo.
        var oggi = DateOnly.FromDateTime(DateTime.UtcNow);
        IQueryable<Sessione> chiuseQuery = _db.Sessioni.Where(s => s.Chiusa && s.DataRiferimento == data);
        if (data == oggi)
        {
            var imp = await GetConfigAsync();
            if (imp.UltimoResetStatistiche.HasValue)
                chiuseQuery = chiuseQuery.Where(s => s.Chiusura >= imp.UltimoResetStatistiche.Value);
        }

        var incassato = await chiuseQuery
            .SelectMany(s => s.Consumazioni)
            .SumAsync(c => (decimal?)c.Quantita * c.Prodotto.Prezzo) ?? 0;

        var attivi = await _db.Sessioni
            .CountAsync(s => !s.Chiusa && s.DataRiferimento == data && s.OmbrelloneId != null);

        return (aperto, incassato, attivi);
    }

    public async Task ResetVisivoStatisticheAsync()
    {
        var imp = await GetConfigAsync();
        imp.UltimoResetStatistiche = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
