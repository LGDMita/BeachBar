using BeachBar.Core.Entities;
using BeachBar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BeachBar.Infrastructure.Services;

public class ConsumazioniService : IConsumazioniService
{
    private readonly BeachBarDbContext _db;

    public ConsumazioniService(BeachBarDbContext db) => _db = db;

    public async Task AggiungiConsumazione(int sessioneId, int prodottoId, DateOnly giorno)
    {
        var consumazione = await _db.Consumazioni
            .FirstOrDefaultAsync(c => c.SessioneId == sessioneId && c.ProdottoId == prodottoId && c.Giorno == giorno);

        if (consumazione != null)
            consumazione.Quantita++;
        else
            _db.Consumazioni.Add(new Consumazione
            {
                SessioneId = sessioneId,
                ProdottoId = prodottoId,
                Quantita = 1,
                Timestamp = DateTime.UtcNow,
                Giorno = giorno
            });

        await _db.SaveChangesAsync();
    }

    public async Task<Consumazione> AggiungiConsumazioneConQuantitaAsync(int sessioneId, int prodottoId, int quantita)
    {
        // Giorno incluso nel lookup: senza di esso un prodotto del giorno precedente (soggiorno
        // multi-giorno) verrebbe incrementato invece di crearne uno nuovo per oggi.
        var oggi = DateOnly.FromDateTime(DateTime.Today);
        var consumazione = await _db.Consumazioni
            .FirstOrDefaultAsync(c => c.SessioneId == sessioneId && c.ProdottoId == prodottoId && c.Giorno == oggi);

        if (consumazione != null)
        {
            consumazione.Quantita += quantita;
        }
        else
        {
            consumazione = new Consumazione
            {
                SessioneId = sessioneId,
                ProdottoId = prodottoId,
                Quantita = quantita,
                Timestamp = DateTime.UtcNow,
                Giorno = oggi
            };
            _db.Consumazioni.Add(consumazione);
        }

        await _db.SaveChangesAsync();
        // Carichiamo la navigation property Prodotto per poter costruire il DTO nella risposta.
        await _db.Entry(consumazione).Reference(c => c.Prodotto).LoadAsync();
        return consumazione;
    }

    public async Task RimuoviConsumazione(int sessioneId, int prodottoId, DateOnly giorno)
    {
        var consumazione = await _db.Consumazioni
            .FirstOrDefaultAsync(c => c.SessioneId == sessioneId && c.ProdottoId == prodottoId && c.Giorno == giorno);
        if (consumazione == null) return;

        if (consumazione.Quantita > 1)
            consumazione.Quantita--;
        else
            _db.Consumazioni.Remove(consumazione);

        await _db.SaveChangesAsync();
    }

    public async Task<bool> EliminaConsumazioneByIdAsync(int consumazioneId)
    {
        var consumazione = await _db.Consumazioni.FindAsync(consumazioneId);
        if (consumazione == null) return false;
        _db.Consumazioni.Remove(consumazione);
        await _db.SaveChangesAsync();
        return true;
    }
}
