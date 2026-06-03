namespace BeachBar.Services;

/// <summary>
/// Mantiene la data di lavoro selezionata dall'utente per tutta la sessione Blazor.
/// Registrato come Scoped (non Singleton) perché ogni circuito SignalR (ogni tab del browser)
/// deve avere la propria data indipendente: un Singleton condividerebbe la data tra tutti gli utenti.
/// </summary>
public class DateContext
{
    public DateOnly DataSelezionata { get; private set; } = DateOnly.FromDateTime(DateTime.Today);

    public void Imposta(DateOnly data)
    {
        DataSelezionata = data;
    }

    public void Avanza() => DataSelezionata = DataSelezionata.AddDays(1);
    public void Arretra() => DataSelezionata = DataSelezionata.AddDays(-1);
    public void TornaOggi() => DataSelezionata = DateOnly.FromDateTime(DateTime.Today);

    public bool IsOggi => DataSelezionata == DateOnly.FromDateTime(DateTime.Today);
}
