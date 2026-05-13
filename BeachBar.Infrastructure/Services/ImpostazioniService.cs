using BeachBar.Core.Entities;
using BeachBar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BeachBar.Infrastructure.Services;

public class ImpostazioniService : IImpostazioniService
{
    private readonly BeachBarDbContext _db;

    public ImpostazioniService(BeachBarDbContext db) => _db = db;

    public async Task<ImpostazioniSpiaggia> GetImpostazioniAsync()
        => await _db.ImpostazioniSpiaggia.FirstAsync(i => i.Id == 1);

    public async Task AggiornaColumeAsync(int colonne)
    {
        var imp = await _db.ImpostazioniSpiaggia.FirstAsync(i => i.Id == 1);
        imp.NumeroColonne = colonne;
        await _db.SaveChangesAsync();
    }

    public async Task AggiornaOmbrelloniAsync(int numero)
    {
        var imp = await _db.ImpostazioniSpiaggia.FirstAsync(i => i.Id == 1);
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

    public async Task<(decimal aperto, decimal incassato, int ombrelloniAttivi)> GetStatisticheAsync()
    {
        var imp = await GetImpostazioniAsync();
        var dataInizioFiltro = imp.UltimoResetStatistiche ?? DateTime.UtcNow.Date;

        var aperto = await _db.Sessioni
            .Where(s => !s.Chiusa)
            .SelectMany(s => s.Consumazioni)
            .SumAsync(c => (decimal?)c.Quantita * c.Prodotto.Prezzo) ?? 0;

        var incassato = await _db.Sessioni
            .Where(s => s.Chiusa && s.Chiusura >= dataInizioFiltro)
            .SelectMany(s => s.Consumazioni)
            .SumAsync(c => (decimal?)c.Quantita * c.Prodotto.Prezzo) ?? 0;

        var attivi = await _db.Ombrelloni.CountAsync(o => o.Occupato);

        return (aperto, incassato, attivi);
    }

    public async Task ResetVisivoStatisticheAsync()
    {
        var imp = await _db.ImpostazioniSpiaggia.FirstAsync(i => i.Id == 1);
        imp.UltimoResetStatistiche = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
