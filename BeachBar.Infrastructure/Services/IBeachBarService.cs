using BeachBar.Core.Entities;

namespace BeachBar.Infrastructure.Services;

public interface IBeachBarService
{
    // Ombrelloni
    Task<List<Ombrellone>> GetOmbrelloniAsync();
    Task<Ombrellone?> GetOmbrelloneByIdAsync(int id);
    Task AggiornaOmbrelloniAsync(int numero);

    // Prodotti
    Task<List<Prodotto>> GetProdottiAsync();
    Task<List<Prodotto>> GetTuttiProdottiAsync();
    Task<Prodotto?> GetProdottoByIdAsync(int id);
    Task<List<Prodotto>> GetProdottiPerCategoriaAsync(string categoria);
    Task<List<string>> GetCategorieAsync();
    Task AggiuntaProdottoAsync(string nome, decimal prezzo, string categoria);
    Task AggiornaProdottoAsync(int id, string nome, decimal prezzo, string categoria, bool disponibile);
    Task EliminaProdottoAsync(int id);
    Task RinominaCategoriaAsync(string vecchia, string nuova);
    Task EliminaCategoriaAsync(string categoria);

    // Sessioni
    Task<List<Sessione>> GetTutteSessioniAsync();
    Task<List<Sessione>> GetSessioniAperteAsync();
    Task<Sessione?> GetSessioneByIdAsync(int id);
    Task<Sessione?> GetSessioneAttivaAsync(int ombrelloneId);
    Task<List<Sessione>> GetStoricoSessioniAsync();
    Task ApriSessioneAsync(int ombrelloneId, string? nomeCliente);
    Task ChiudiSessioneAsync(int sessioneId);
    Task AnnullaSessioneAsync(int sessioneId);
    Task AggiornaNomeSessioneAsync(int sessioneId, string? nuovoNome);
    Task EliminaSessioneStoricoAsync(int id);
    Task ResetTotaliAsync();

    // Consumazioni
    Task AggiungiConsumazione(int sessioneId, int prodottoId);
    Task<Consumazione> AggiungiConsumazioneConQuantitaAsync(int sessioneId, int prodottoId, int quantita);
    Task RimuoviConsumazione(int sessioneId, int prodottoId);
    Task<bool> EliminaConsumazioneByIdAsync(int consumazioneId);

    // Impostazioni e statistiche
    Task<ImpostazioniSpiaggia> GetImpostazioniAsync();
    Task AggiornaColumeAsync(int colonne);
    Task<(decimal aperto, decimal incassato, int ombrelloniAttivi)> GetStatisticheAsync();
    Task ResetVisivoStatisticheAsync();
}
