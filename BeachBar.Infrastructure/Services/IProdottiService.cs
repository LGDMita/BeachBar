using BeachBar.Core.Entities;

namespace BeachBar.Infrastructure.Services;

public interface IProdottiService
{
    /// <summary>Restituisce solo i prodotti con Disponibile = true.</summary>
    Task<List<Prodotto>> GetProdottiAsync();

    /// <summary>Restituisce tutti i prodotti inclusi quelli non disponibili.</summary>
    Task<List<Prodotto>> GetTuttiProdottiAsync();

    /// <summary>Restituisce un prodotto per ID, o null se non esiste.</summary>
    Task<Prodotto?> GetProdottoByIdAsync(int id);

    /// <summary>Restituisce i prodotti di una categoria specifica.</summary>
    Task<List<Prodotto>> GetProdottiPerCategoriaAsync(string categoria);

    /// <summary>Restituisce l'elenco delle categorie distinte.</summary>
    Task<List<string>> GetCategorieAsync();

    Task AggiuntaProdottoAsync(string nome, decimal prezzo, string categoria);
    Task AggiornaProdottoAsync(int id, string nome, decimal prezzo, string categoria, bool disponibile);
    Task EliminaProdottoAsync(int id);
    Task RinominaCategoriaAsync(string vecchia, string nuova);
    Task EliminaCategoriaAsync(string categoria);
}
