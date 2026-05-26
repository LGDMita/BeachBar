using BeachBar.Core.Entities;

namespace BeachBar.Infrastructure.Services;

public interface ISessioniService
{
    /// <summary>Restituisce gli ombrelloni con tutte le sessioni aperte per la data specificata.</summary>
    Task<List<Ombrellone>> GetOmbrelloniAsync(DateOnly data);

    /// <summary>Restituisce un ombrellone per ID, o null se non esiste.</summary>
    Task<Ombrellone?> GetOmbrelloneByIdAsync(int id);

    /// <summary>Restituisce tutte le sessioni (aperte e chiuse) con consumazioni incluse.</summary>
    Task<List<Sessione>> GetTutteSessioniAsync();

    /// <summary>Restituisce solo le sessioni attualmente aperte.</summary>
    Task<List<Sessione>> GetSessioniAperteAsync();

    /// <summary>Restituisce una sessione per ID con ombrellone e consumazioni inclusi, o null se non esiste.</summary>
    Task<Sessione?> GetSessioneByIdAsync(int id);

    /// <summary>Restituisce tutte le sessioni aperte di un ombrellone per la data specificata.</summary>
    Task<List<Sessione>> GetSessioniAttivePerOmbrelloneAsync(int ombrelloneId, DateOnly data);

    /// <summary>Restituisce la prima sessione aperta di un ombrellone per la data specificata (usato dall'API).</summary>
    Task<Sessione?> GetSessioneAttivaAsync(int ombrelloneId, DateOnly data);

    /// <summary>Restituisce tutte le sessioni aperte senza ombrellone (conti extra/volanti) per la data specificata.</summary>
    Task<List<Sessione>> GetContiExtraAsync(DateOnly data);

    /// <summary>Restituisce lo storico delle sessioni chiuse per la data specificata, dalla più recente.</summary>
    Task<List<Sessione>> GetStoricoSessioniAsync(DateOnly filtroData);

    /// <summary>Apre una nuova sessione su un ombrellone per la data specificata. Restituisce la sessione creata.</summary>
    Task<Sessione> ApriSessioneAsync(int ombrelloneId, string? nomeCliente, DateOnly dataRiferimento);

    /// <summary>Apre un conto extra senza ombrellone (ospite volante). Restituisce la sessione creata.</summary>
    Task<Sessione> ApriContoExtraAsync(string? nome, DateOnly dataRiferimento);

    /// <summary>Chiude la sessione, registra la data di chiusura e libera l'ombrellone se non ha altre sessioni aperte oggi.</summary>
    Task ChiudiSessioneAsync(int sessioneId);

    Task AnnullaSessioneAsync(int sessioneId);
    Task AggiornaNomeSessioneAsync(int sessioneId, string? nuovoNome);
    Task EliminaSessioneStoricoAsync(int id);

    /// <summary>Chiude forzatamente tutte le sessioni aperte. Usato per il reset giornaliero.</summary>
    Task ResetTotaliAsync();
}
