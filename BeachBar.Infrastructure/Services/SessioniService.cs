using BeachBar.Core.Entities;
using BeachBar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BeachBar.Infrastructure.Services;

public class SessioniService : ISessioniService
{
    private readonly BeachBarDbContext _db;

    public SessioniService(BeachBarDbContext db) => _db = db;

    public async Task<List<Ombrellone>> GetOmbrelloniAsync(DateOnly data)
    {
        var ombrelloni = await _db.Ombrelloni
            .AsNoTracking()
            .OrderBy(o => o.Numero)
            .ToListAsync();

        // Sessioni attive per la data: include soggiorni multi-giorno che comprendono questa data
        var sessioniAperte = await _db.Sessioni
            .AsNoTracking()
            .Where(s => !s.Chiusa && s.OmbrelloneId != null
                     && s.DataRiferimento <= data
                     && (s.DataFine == null || s.DataFine >= data))
            .ToListAsync();

        var perOmbrellone = sessioniAperte
            .GroupBy(s => s.OmbrelloneId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var o in ombrelloni)
        {
            if (perOmbrellone.TryGetValue(o.Id, out var sessioni))
            {
                o.Occupato = true;
                o.Sessioni = sessioni;
            }
            else
            {
                o.Occupato = false;
                o.Sessioni = new List<Sessione>();
            }
        }

        return ombrelloni;
    }

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

    public async Task<List<Sessione>> GetSessioniAttivePerOmbrelloneAsync(int ombrelloneId, DateOnly data)
        => await _db.Sessioni
            .Include(s => s.Consumazioni).ThenInclude(c => c.Prodotto)
            .Where(s => s.OmbrelloneId == ombrelloneId && !s.Chiusa
                     && s.DataRiferimento <= data
                     && (s.DataFine == null || s.DataFine >= data))
            .OrderBy(s => s.Apertura)
            .ToListAsync();

    public async Task<Sessione?> GetSessioneAttivaAsync(int ombrelloneId, DateOnly data)
        => await _db.Sessioni
            .Include(s => s.Consumazioni).ThenInclude(c => c.Prodotto)
            .FirstOrDefaultAsync(s => s.OmbrelloneId == ombrelloneId && !s.Chiusa && s.DataRiferimento == data);

    public async Task<List<Sessione>> GetContiExtraAsync(DateOnly data)
        => await _db.Sessioni
            .Include(s => s.Consumazioni).ThenInclude(c => c.Prodotto)
            .Where(s => s.OmbrelloneId == null && !s.Chiusa && s.DataRiferimento == data)
            .OrderBy(s => s.Apertura)
            .ToListAsync();

    public async Task<List<Sessione>> GetStoricoSessioniAsync(DateOnly filtroData)
        => await _db.Sessioni
            .Include(s => s.Consumazioni).ThenInclude(c => c.Prodotto)
            .Include(s => s.Ombrellone)
            .Where(s => s.Chiusa && s.DataRiferimento == filtroData)
            .OrderByDescending(s => s.Chiusura)
            .ToListAsync();

    public async Task<Sessione> ApriSessioneAsync(int ombrelloneId, string? nomeCliente, DateOnly dataRiferimento, int giorni = 1)
    {
        var ombrellone = await _db.Ombrelloni.FindAsync(ombrelloneId)
            ?? throw new InvalidOperationException($"Ombrellone {ombrelloneId} non trovato.");

        var oggi = DateOnly.FromDateTime(DateTime.UtcNow);
        if (dataRiferimento <= oggi && (giorni <= 1 || dataRiferimento.AddDays(giorni - 1) >= oggi))
            ombrellone.Occupato = true;

        var dataFine = giorni > 1 ? dataRiferimento.AddDays(giorni - 1) : (DateOnly?)null;

        var nuova = new Sessione
        {
            OmbrelloneId = ombrelloneId,
            NomeCliente = nomeCliente,
            Apertura = DateTime.UtcNow,
            Chiusa = false,
            DataRiferimento = dataRiferimento,
            DataFine = dataFine
        };
        _db.Sessioni.Add(nuova);
        await _db.SaveChangesAsync();

        await _db.Entry(nuova).Reference(s => s.Ombrellone).LoadAsync();
        nuova.Consumazioni = new List<Consumazione>();
        return nuova;
    }

    public async Task<Sessione> ApriContoExtraAsync(string? nome, DateOnly dataRiferimento)
    {
        var nuova = new Sessione
        {
            OmbrelloneId = null,
            NomeCliente = nome,
            Apertura = DateTime.UtcNow,
            Chiusa = false,
            DataRiferimento = dataRiferimento
        };
        _db.Sessioni.Add(nuova);
        await _db.SaveChangesAsync();

        nuova.Consumazioni = new List<Consumazione>();
        return nuova;
    }

    public async Task ChiudiSessioneAsync(int sessioneId)
    {
        var sessione = await _db.Sessioni
            .Include(s => s.Ombrellone)
            .FirstOrDefaultAsync(s => s.Id == sessioneId)
            ?? throw new InvalidOperationException($"Sessione {sessioneId} non trovata.");

        sessione.Chiusa = true;
        sessione.Chiusura = DateTime.UtcNow;

        // Libera il flag Occupato solo se è oggi e non ci sono altre sessioni aperte sullo stesso ombrellone
        if (sessione.OmbrelloneId.HasValue && sessione.DataRiferimento == DateOnly.FromDateTime(DateTime.UtcNow))
        {
            var altreAperte = await _db.Sessioni.AnyAsync(s =>
                s.OmbrelloneId == sessione.OmbrelloneId && !s.Chiusa && s.Id != sessioneId);
            if (!altreAperte)
                sessione.Ombrellone!.Occupato = false;
        }

        await _db.SaveChangesAsync();
    }

    public async Task AnnullaSessioneAsync(int sessioneId)
    {
        var sessione = await _db.Sessioni
            .Include(s => s.Ombrellone)
            .FirstOrDefaultAsync(s => s.Id == sessioneId)
            ?? throw new InvalidOperationException($"Sessione {sessioneId} non trovata.");

        bool eraOggi = sessione.DataRiferimento == DateOnly.FromDateTime(DateTime.UtcNow);
        int? ombrelloneId = sessione.OmbrelloneId;
        var ombrellone = sessione.Ombrellone;

        // Controlla prima della rimozione se ci sono altre sessioni aperte (esclusa questa)
        bool liberaOmbrellone = false;
        if (ombrelloneId.HasValue && eraOggi)
        {
            var altreAperte = await _db.Sessioni.AnyAsync(s =>
                s.OmbrelloneId == ombrelloneId && !s.Chiusa && s.Id != sessioneId);
            liberaOmbrellone = !altreAperte;
        }

        if (liberaOmbrellone && ombrellone != null)
            ombrellone.Occupato = false;

        _db.Sessioni.Remove(sessione);
        await _db.SaveChangesAsync();
    }

    public async Task AggiornaNomeSessioneAsync(int sessioneId, string? nuovoNome)
    {
        var sessione = await _db.Sessioni.FindAsync(sessioneId)
            ?? throw new InvalidOperationException($"Sessione {sessioneId} non trovata.");

        sessione.NomeCliente = nuovoNome;
        await _db.SaveChangesAsync();
    }

    public async Task EliminaSessioneStoricoAsync(int id)
    {
        var s = await _db.Sessioni.FindAsync(id)
            ?? throw new InvalidOperationException($"Sessione {id} non trovata nello storico.");

        _db.Sessioni.Remove(s);
        await _db.SaveChangesAsync();
    }

    public async Task ResetTotaliAsync()
    {
        var sessioniAperte = await _db.Sessioni
            .Include(s => s.Ombrellone)
            .Where(s => !s.Chiusa)
            .ToListAsync();

        var oggi = DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var s in sessioniAperte)
        {
            s.Chiusa = true;
            s.Chiusura = DateTime.UtcNow;
            if (s.OmbrelloneId.HasValue && s.DataRiferimento == oggi)
                s.Ombrellone!.Occupato = false;
        }

        await _db.SaveChangesAsync();
    }
}
