using BeachBar.Core.Entities;
using BeachBar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BeachBar.Infrastructure.Services;

public class BeachBarService
{
    private readonly BeachBarDbContext _db;

    public BeachBarService(BeachBarDbContext db)
    {
        _db = db;
    }

    public async Task<List<Ombrellone>> GetOmbrelloniAsync()
        => await _db.Ombrelloni.OrderBy(o => o.Numero).ToListAsync();

    public async Task<List<Prodotto>> GetProdottiAsync()
        => await _db.Prodotti.Where(p => p.Disponibile).OrderBy(p => p.Nome).ToListAsync();

    public async Task<Sessione?> GetSessioneAttivaAsync(int ombrelloneId)
        => await _db.Sessioni
            .Include(s => s.Consumazioni)
            .ThenInclude(c => c.Prodotto)
            .FirstOrDefaultAsync(s => s.OmbrelloneId == ombrelloneId && !s.Chiusa);

    public async Task ApriSessioneAsync(int ombrelloneId, string? nomeCliente)
    {
        var ombrellone = await _db.Ombrelloni.FindAsync(ombrelloneId);
        if (ombrellone == null) return;

        var sessione = new Sessione
        {
            OmbrelloneId = ombrelloneId,
            NomeCliente = nomeCliente,
            Apertura = DateTime.UtcNow,
            Chiusa = false
        };

        ombrellone.Occupato = true;
        _db.Sessioni.Add(sessione);
        await _db.SaveChangesAsync();
    }

    public async Task AggiungiConsumazione(int sessioneId, int prodottoId)
    {
        var consumazione = await _db.Consumazioni
            .FirstOrDefaultAsync(c => c.SessioneId == sessioneId && c.ProdottoId == prodottoId);

        if (consumazione != null)
        {
            consumazione.Quantita++;
        }
        else
        {
            _db.Consumazioni.Add(new Consumazione
            {
                SessioneId = sessioneId,
                ProdottoId = prodottoId,
                Quantita = 1,
                Timestamp = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
    }

    public async Task ChiudiSessioneAsync(int sessioneId)
    {
        var sessione = await _db.Sessioni
            .Include(s => s.Ombrellone)
            .FirstOrDefaultAsync(s => s.Id == sessioneId);
        if (sessione == null) return;

        sessione.Chiusa = true;
        sessione.Chiusura = DateTime.UtcNow;
        sessione.Ombrellone.Occupato = false;
        await _db.SaveChangesAsync();
    }

    public async Task RimuoviConsumazione(int sessioneId, int prodottoId)
    {
        var consumazione = await _db.Consumazioni
            .FirstOrDefaultAsync(c => c.SessioneId == sessioneId && c.ProdottoId == prodottoId);

        if (consumazione == null) return;

        if (consumazione.Quantita > 1)
            consumazione.Quantita--;
        else
            _db.Consumazioni.Remove(consumazione);

        await _db.SaveChangesAsync();
    }

    public async Task<(decimal aperto, decimal incassato, int ombrelloniAttivi)> GetStatisticheAsync()
    {
        var oggi = DateTime.UtcNow.Date;

        var aperto = await _db.Sessioni
            .Where(s => !s.Chiusa)
            .SelectMany(s => s.Consumazioni)
            .SumAsync(c => c.Quantita * c.Prodotto.Prezzo);

        var incassato = await _db.Sessioni
            .Where(s => s.Chiusa && s.Chiusura.HasValue && s.Chiusura.Value.Date == oggi)
            .SelectMany(s => s.Consumazioni)
            .SumAsync(c => c.Quantita * c.Prodotto.Prezzo);

        var attivi = await _db.Ombrelloni.CountAsync(o => o.Occupato);

        return (aperto, incassato, attivi);
    }
}