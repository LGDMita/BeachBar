using BeachBar.Core.Entities;

namespace BeachBar.Infrastructure.Services;

public interface IConsumazioniService
{
    /// <summary>
    /// Aggiunge 1 unità di un prodotto alla sessione per il giorno specificato.
    /// Se il prodotto esiste già per quel giorno, incrementa la quantità.
    /// </summary>
    Task AggiungiConsumazione(int sessioneId, int prodottoId, DateOnly giorno);

    /// <summary>
    /// Aggiunge una quantità specificata di un prodotto alla sessione.
    /// Se il prodotto è già presente, somma la quantità a quella esistente.
    /// Restituisce la consumazione aggiornata. Usato dall'API REST.
    /// </summary>
    Task<Consumazione> AggiungiConsumazioneConQuantitaAsync(int sessioneId, int prodottoId, int quantita);

    /// <summary>Rimuove 1 unità di un prodotto per il giorno specificato; se la quantità arriva a 0 elimina la riga.</summary>
    Task RimuoviConsumazione(int sessioneId, int prodottoId, DateOnly giorno);

    /// <summary>
    /// Elimina una consumazione per ID diretto.
    /// Restituisce false se la consumazione non esiste.
    /// Usato dall'API REST per la DELETE /ordini/{ordineId}.
    /// </summary>
    Task<bool> EliminaConsumazioneByIdAsync(int consumazioneId);
}
