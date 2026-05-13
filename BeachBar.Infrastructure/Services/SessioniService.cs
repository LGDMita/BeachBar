using BeachBar.Core.Entities;
using BeachBar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BeachBar.Infrastructure.Services;

public class SessioniService : ISessioniService
{
    private readonly BeachBarDbContext _db;

    public SessioniService(BeachBarDbContext db) => _db = db;

    public async Task<List<Ombrellone>> GetOmbrelloniAsync()
        => await _db.Ombrelloni.OrderBy(o => o.Numero).ToListAsync();

    public async Task<Ombrellone?> GetOmbrelloneByIdAsync(int id)
        => await _db.Ombrelloni.FindAsync(id);

    public async Task<List<Sessione>> GetTutteSessioniAsync()
        => await _db.Sessioni
            .Include(s => s.Ombrellone)
            .Include(s => s.Consumazioni).ThenInclude(c => c.Prodotto)
            .OrderByDescending(s => s.Apertura)
            .ToListAsync();

    public async Task<List<Sessione>> GetSessioniAperteAsync()
        => await _db.Sessioni
            .Include(s => s.Ombrellone)
            .Include(s => s.Consumazioni).ThenInclude(c => c.Prodotto)
            .Where(s => !s.Chiusa)
            .OrderBy(s => s.Apertura)
            .ToListAsync();

    public async Task<Sessione?> GetSessioneByIdAsync(int id)
        => await _db.Sessioni
            .Include(s => s.Ombrellone)
            .Include(s => s.Consumazioni).ThenInclude(c => c.Prodotto)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<Sessione?> GetSessioneAttivaAsync(int ombrelloneId)
        => await _db.Sessioni
            .Include(s => s.Consumazioni).ThenInclude(c => c.Prodotto)
            .FirstOrDefaultAsync(s => s.OmbrelloneId == ombrelloneId && !s.Chiusa);

    public async Task<List<Sessione>> GetStoricoSessioniAsync()
        => await _db.Sessioni
            .Include(s => s.Consumazioni).ThenInclude(c => c.Prodotto)
            .Include(s => s.Ombrellone)
            .Where(s => s.Chiusa)
            .OrderByDescending(s => s.Chiusura)
            .ToListAsync();

    public async Task ApriSessioneAsync(int ombrelloneId, string? nomeCliente)
    {
        var ombrellone = await _db.Ombrelloni.FindAsync(ombrelloneId);
        if (ombrellone == null) return;

        ombrellone.Occupato = true;
        _db.Sessioni.Add(new Sessione
        {
            OmbrelloneId = ombrelloneId,
            NomeCliente = nomeCliente,
            Apertura = DateTime.UtcNow,
            Chiusa = false
        });
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

    public async Task AnnullaSessioneAsync(int sessioneId)
    {
        var sessione = await _db.Sessioni
            .Include(s => s.Ombrellone)
            .FirstOrDefaultAsync(s => s.Id == sessioneId);
        if (sessione == null) return;

        if (sessione.Ombrellone != null)
            sessione.Ombrellone.Occupato = false;

        _db.Sessioni.Remove(sessione);
        await _db.SaveChangesAsync();
    }

    public async Task AggiornaNomeSessioneAsync(int sessioneId, string? nuovoNome)
    {
        var sessione = await _db.Sessioni.FindAsync(sessioneId);
        if (sessione != null)
        {
            sessione.NomeCliente = nuovoNome;
            await _db.SaveChangesAsync();
        }
    }

    public async Task EliminaSessioneStoricoAsync(int id)
    {
        var s = await _db.Sessioni.FindAsync(id);
        if (s != null) { _db.Sessioni.Remove(s); await _db.SaveChangesAsync(); }
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
}
