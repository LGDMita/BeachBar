using BeachBar.Core.Entities;

namespace BeachBar.Infrastructure.Services;

public interface ISessioniService
{
    /// <summary>Restituisce tutti gli ombrelloni ordinati per numero.</summary>
    Task<List<Ombrellone>> GetOmbrelloniAsync();

    /// <summary>Restituisce un ombrellone per ID, o null se non esiste.</summary>
    Task<Ombrellone?> GetOmbrelloneByIdAsync(int id);

    /// <summary>Restituisce tutte le sessioni (aperte e chiuse) con consumazioni incluse.</summary>
    Task<List<Sessione>> GetTutteSessioniAsync();

    /// <summary>Restituisce solo le sessioni attualmente aperte.</summary>
    Task<List<Sessione>> GetSessioniAperteAsync();

    /// <summary>Restituisce una sessione per ID con ombrellone e consumazioni inclusi, o null se non esiste.</summary>
    Task<Sessione?> GetSessioneByIdAsync(int id);

    /// <summary>Restituisce la sessione aperta di un ombrellone, o null se l'ombrellone è libero.</summary>
    Task<Sessione?> GetSessioneAttivaAsync(int ombrelloneId);

    /// <summary>Restituisce lo storico delle sessioni chiuse, dalla più recente.</summary>
    Task<List<Sessione>> GetStoricoSessioniAsync();

    /// <summary>Apre una nuova sessione e segna l'ombrellone come occupato.</summary>
    Task ApriSessioneAsync(int ombrelloneId, string? nomeCliente);

    /// <summary>Chiude la sessione, registra la data di chiusura e libera l'ombrellone.</summary>
    Task ChiudiSessioneAsync(int sessioneId);

    Task AnnullaSessioneAsync(int sessioneId);
    Task AggiornaNomeSessioneAsync(int sessioneId, string? nuovoNome);
    Task EliminaSessioneStoricoAsync(int id);

    /// <summary>Chiude forzatamente tutte le sessioni aperte. Usato per il reset giornaliero.</summary>
    Task ResetTotaliAsync();
}
