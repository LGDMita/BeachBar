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

    /// <summary>Restituisce gli ID degli ombrelloni che hanno almeno una consumazione su una sessione aperta per la data specificata. Usato per distinguere "con lista" (rosso) da "senza lista" (giallo).</summary>
    Task<HashSet<int>> GetUmbrelleConProdottiAsync(DateOnly data);

    /// <summary>Restituisce lo storico delle sessioni chiuse per la data specificata, dalla più recente.</summary>
    Task<List<Sessione>> GetStoricoSessioniAsync(DateOnly filtroData);

    /// <summary>Apre una nuova lista su un ombrellone. giorni > 1 imposta DataFine per soggiorni multi-giorno.</summary>
    Task<Sessione> ApriSessioneAsync(int ombrelloneId, string? nomeCliente, DateOnly dataRiferimento, int giorni = 1);

    /// <summary>Apre un conto extra senza ombrellone (ospite volante). Restituisce la sessione creata.</summary>
    Task<Sessione> ApriContoExtraAsync(string? nome, DateOnly dataRiferimento);

    /// <summary>
    /// Chiude la sessione e registra la data di chiusura.
    /// Se <paramref name="liberaOmbrellone"/> è true, imposta Occupato=false sull'ombrellone
    /// (solo se non ci sono altre sessioni aperte). Se false, l'ombrellone resta occupato
    /// anche senza sessioni aperte — utile per clienti fissi che pagano ogni giorno.
    /// </summary>
    Task ChiudiSessioneAsync(int sessioneId, bool liberaOmbrellone = false);

    /// <summary>Elimina la sessione e, se non ci sono altre sessioni chiuse oggi, libera l'ombrellone.</summary>
    Task AnnullaSessioneAsync(int sessioneId);

    /// <summary>Aggiorna solo il nome del cliente sulla sessione, senza toccare lo stato.</summary>
    Task AggiornaNomeSessioneAsync(int sessioneId, string? nuovoNome);

    /// <summary>Elimina definitivamente una sessione già chiusa dallo storico.</summary>
    Task EliminaSessioneStoricoAsync(int id);

    /// <summary>Libera manualmente l'ombrellone (Occupato=false) senza sessioni da chiudere. Usato dalla pagina Ombrellone per ombrelloni "senza lista".</summary>
    Task LiberaOmbrelloneAsync(int ombrelloneId);

    /// <summary>Chiude forzatamente tutte le sessioni aperte. Usato per il reset giornaliero.</summary>
    Task ResetTotaliAsync();

    /// <summary>Restituisce i giorni già coperti da una sessione aperta sull'ombrellone nell'intervallo [dal, al].</summary>
    Task<HashSet<DateOnly>> GetGiorniOccupatiAsync(int ombrelloneId, DateOnly dal, DateOnly al);
}
