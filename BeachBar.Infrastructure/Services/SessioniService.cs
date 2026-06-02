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

        var oggi = DateOnly.FromDateTime(DateTime.Today);
        foreach (var o in ombrelloni)
        {
            if (perOmbrellone.TryGetValue(o.Id, out var sessioni))
            {
                o.Occupato = true;
                o.Sessioni = sessioni;
            }
            else
            {
                // Per date diverse da oggi: nessuna sessione = libero.
                // Per oggi: preserva il flag DB — il cliente può essere "senza lista".
                if (data != oggi)
                    o.Occupato = false;
                o.Sessioni = new List<Sessione>();
            }
        }

        // Per gli ombrelloni "senza lista" di oggi, carica il nome dal conto chiuso più recente
        // così il nome del cliente rimane visibile in dashboard anche dopo "Chiudi lista e incassa".
        if (data == oggi)
        {
            var senzaListaIds = ombrelloni
                .Where(o => o.Occupato && !perOmbrellone.ContainsKey(o.Id))
                .Select(o => o.Id)
                .ToList();

            if (senzaListaIds.Any())
            {
                var sessioniChiuseOggi = await _db.Sessioni
                    .AsNoTracking()
                    .Where(s => s.OmbrelloneId != null
                             && senzaListaIds.Contains(s.OmbrelloneId!.Value)
                             && s.Chiusa
                             && s.DataRiferimento == oggi)
                    .OrderByDescending(s => s.Chiusura)
                    .Select(s => new { s.OmbrelloneId, s.NomeCliente })
                    .ToListAsync();

                var nomiPerOmbrellone = sessioniChiuseOggi
                    .GroupBy(s => s.OmbrelloneId!.Value)
                    .ToDictionary(g => g.Key, g => g.First().NomeCliente);

                foreach (var o in ombrelloni)
                    if (nomiPerOmbrellone.TryGetValue(o.Id, out var nome))
                        o.NomeClienteAttivo = nome;
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

    public async Task<HashSet<int>> GetUmbrelleConProdottiAsync(DateOnly data)
    {
        var ids = await _db.Sessioni
            .AsNoTracking()
            .Where(s => !s.Chiusa && s.OmbrelloneId != null
                     && s.DataRiferimento <= data
                     && (s.DataFine == null || s.DataFine >= data)
                     && s.Consumazioni.Any())
            .Select(s => s.OmbrelloneId!.Value)
            .Distinct()
            .ToListAsync();
        return new HashSet<int>(ids);
    }

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

        var oggi = DateOnly.FromDateTime(DateTime.Today);
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

    public async Task ChiudiSessioneAsync(int sessioneId, bool liberaOmbrellone = false)
    {
        var sessione = await _db.Sessioni
            .Include(s => s.Ombrellone)
            .FirstOrDefaultAsync(s => s.Id == sessioneId)
            ?? throw new InvalidOperationException($"Sessione {sessioneId} non trovata.");

        sessione.Chiusa = true;
        sessione.Chiusura = DateTime.UtcNow;

        if (liberaOmbrellone && sessione.OmbrelloneId.HasValue)
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

        var oggi = DateOnly.FromDateTime(DateTime.Today);
        // "Affetta oggi" = la sessione è attiva nel giorno corrente (gestisce sia singolo che multi-giorno)
        bool affettaOggi = sessione.DataRiferimento <= oggi
                        && (sessione.DataFine == null || sessione.DataFine >= oggi);

        int? ombrelloneId = sessione.OmbrelloneId;
        var ombrellone = sessione.Ombrellone;

        bool liberaOmbrellone = false;
        if (ombrelloneId.HasValue && affettaOggi)
        {
            var altreAperte = await _db.Sessioni.AnyAsync(s =>
                s.OmbrelloneId == ombrelloneId && !s.Chiusa && s.Id != sessioneId);
            if (!altreAperte)
            {
                // Non liberare se esistono sessioni chiuse oggi: il cliente aveva già usato
                // "Chiudi lista" e l'ombrellone deve restare occupato (cliente presente senza conto).
                var altreChiuseOggi = await _db.Sessioni.AnyAsync(s =>
                    s.OmbrelloneId == ombrelloneId && s.Chiusa
                    && s.DataRiferimento == oggi
                    && s.Id != sessioneId);
                liberaOmbrellone = !altreChiuseOggi;
            }
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

    public async Task LiberaOmbrelloneAsync(int ombrelloneId)
    {
        var o = await _db.Ombrelloni.FindAsync(ombrelloneId)
            ?? throw new InvalidOperationException($"Ombrellone {ombrelloneId} non trovato.");
        o.Occupato = false;
        await _db.SaveChangesAsync();
    }

    public async Task ResetTotaliAsync()
    {
        var sessioniAperte = await _db.Sessioni
            .Where(s => !s.Chiusa)
            .ToListAsync();

        var chiusura = DateTime.UtcNow;
        foreach (var s in sessioniAperte)
        {
            s.Chiusa = true;
            s.Chiusura = chiusura;
        }

        var ombrelloniOccupati = await _db.Ombrelloni
            .Where(o => o.Occupato)
            .ToListAsync();
        foreach (var o in ombrelloniOccupati)
            o.Occupato = false;

        await _db.SaveChangesAsync();
    }
}
