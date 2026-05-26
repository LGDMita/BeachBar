using BeachBar.Core.Entities;

namespace BeachBar.Infrastructure.Services;

public interface IImpostazioniService
{
    Task<ImpostazioniSpiaggia> GetImpostazioniAsync();
    Task AggiornaColumeAsync(int colonne);

    /// <summary>Aggiunge o rimuove ombrelloni per raggiungere il numero desiderato.</summary>
    Task AggiornaOmbrelloniAsync(int numero);

    /// <summary>Calcola il totale aperto, l'incassato e gli ombrelloni attivi per la data specificata.</summary>
    Task<(decimal aperto, decimal incassato, int ombrelloniAttivi)> GetStatisticheAsync(DateOnly data);

    /// <summary>Aggiorna la data di reset visivo per azzerare i contatori della dashboard.</summary>
    Task ResetVisivoStatisticheAsync();
}
