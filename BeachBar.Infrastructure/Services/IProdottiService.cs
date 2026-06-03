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

    /// <summary>Aggiunge un nuovo prodotto con Disponibile=true.</summary>
    Task AggiuntaProdottoAsync(string nome, decimal prezzo, string categoria);

    /// <summary>Aggiorna nome, prezzo, categoria e disponibilità di un prodotto esistente.</summary>
    Task AggiornaProdottoAsync(int id, string nome, decimal prezzo, string categoria, bool disponibile);

    /// <summary>Elimina un prodotto; fallisce se ha consumazioni collegate.</summary>
    Task EliminaProdottoAsync(int id);

    /// <summary>Rinomina una categoria aggiornando tutti i prodotti che la usano.</summary>
    Task RinominaCategoriaAsync(string vecchia, string nuova);

    /// <summary>Elimina tutti i prodotti di una categoria.</summary>
    Task EliminaCategoriaAsync(string categoria);
}
