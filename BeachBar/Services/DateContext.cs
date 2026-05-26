namespace BeachBar.Services;

/// <summary>
/// Mantiene la data di lavoro selezionata dall'utente per tutta la sessione Blazor.
/// Scoped: una istanza per circuito SignalR (un per tab del browser).
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
