namespace BeachBar.Services;

/// <summary>
/// Canale di notifica leggero tra circuiti Blazor Server (tab/tablet diversi).
/// Registrato come Singleton: ogni circuito si sottoscrive all'evento condiviso
/// e riceve un segnale ogni volta che un altro circuito modifica lo stato degli ombrelloni.
/// </summary>
public interface IDashboardEventService
{
    /// <summary>Scatta ogni volta che lo stato degli ombrelloni o delle sessioni cambia.</summary>
    event Action OnStateChanged;

    /// <summary>Notifica tutti i subscriber che lo stato è cambiato.</summary>
    void NotifyStateChanged();
}
