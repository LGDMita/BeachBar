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
            .SumAsync(c => (decimal?)c.Quantita * c.Prodotto.Prezzo) ?? 0;

        var incassato = await _db.Sessioni
            .Where(s => s.Chiusa && s.Chiusura >= oggi)
            .SelectMany(s => s.Consumazioni)
            .SumAsync(c => (decimal?)c.Quantita * c.Prodotto.Prezzo) ?? 0;

        var attivi = await _db.Ombrelloni.CountAsync(o => o.Occupato);

        return (aperto, incassato, attivi);
    }

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

    public async Task ResetTotaliAsync()
    {
        var sessioniAperte = await _db.Sessioni
            .Include(s => s.Ombrellone)
            .Where(s => !s.Chiusa)
            .ToListAsync();

        foreach (var s in sessioniAperte)
        {
            s.Chiusa = true;
            s.Chiusura = DateTime.UtcNow;
            s.Ombrellone.Occupato = false;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<List<Prodotto>> GetTuttiProdottiAsync()
        => await _db.Prodotti.OrderBy(p => p.Categoria).ThenBy(p => p.Nome).ToListAsync();

    public async Task AggiuntaProdottoAsync(string nome, decimal prezzo, string categoria)
    {
        _db.Prodotti.Add(new Prodotto { Nome = nome, Prezzo = prezzo, Categoria = categoria, Disponibile = true });
        await _db.SaveChangesAsync();
    }

    public async Task AggiornaProdottoAsync(int id, string nome, decimal prezzo, string categoria, bool disponibile)
    {
        var p = await _db.Prodotti.FindAsync(id);
        if (p == null) return;
        p.Nome = nome;
        p.Prezzo = prezzo;
        p.Categoria = categoria;
        p.Disponibile = disponibile;
        await _db.SaveChangesAsync();
    }

    public async Task EliminaProdottoAsync(int id)
    {
        var p = await _db.Prodotti.FindAsync(id);
        if (p != null) { _db.Prodotti.Remove(p); await _db.SaveChangesAsync(); }
    }

    public async Task RinominaCategoriaAsync(string vecchia, string nuova)
    {
        var prodotti = await _db.Prodotti.Where(p => p.Categoria == vecchia).ToListAsync();
        foreach (var p in prodotti) p.Categoria = nuova;
        await _db.SaveChangesAsync();
    }

    public async Task EliminaCategoriaAsync(string categoria)
    {
        var prodotti = await _db.Prodotti.Where(p => p.Categoria == categoria).ToListAsync();
        _db.Prodotti.RemoveRange(prodotti);
        await _db.SaveChangesAsync();
    }

    public async Task<List<string>> GetCategorieAsync()
        => await _db.Prodotti.Select(p => p.Categoria).Distinct().OrderBy(c => c).ToListAsync();
}