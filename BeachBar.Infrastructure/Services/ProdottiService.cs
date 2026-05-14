using BeachBar.Core.Entities;
using BeachBar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BeachBar.Infrastructure.Services;

public class ProdottiService : IProdottiService
{
    private readonly BeachBarDbContext _db;

    public ProdottiService(BeachBarDbContext db) => _db = db;

    public async Task<List<Prodotto>> GetProdottiAsync()
        => await _db.Prodotti.Where(p => p.Disponibile).OrderBy(p => p.Nome).ToListAsync();

    public async Task<List<Prodotto>> GetTuttiProdottiAsync()
        => await _db.Prodotti.OrderBy(p => p.Categoria).ThenBy(p => p.Nome).ToListAsync();

    public async Task<Prodotto?> GetProdottoByIdAsync(int id)
        => await _db.Prodotti.FindAsync(id);

    public async Task<List<Prodotto>> GetProdottiPerCategoriaAsync(string categoria)
        => await _db.Prodotti.Where(p => p.Categoria == categoria).OrderBy(p => p.Nome).ToListAsync();

    public async Task<List<string>> GetCategorieAsync()
        => await _db.Prodotti.Select(p => p.Categoria).Distinct().OrderBy(c => c).ToListAsync();

    public async Task AggiuntaProdottoAsync(string nome, decimal prezzo, string categoria)
    {
        _db.Prodotti.Add(new Prodotto { Nome = nome, Prezzo = prezzo, Categoria = categoria, Disponibile = true });
        await _db.SaveChangesAsync();
    }

    public async Task AggiornaProdottoAsync(int id, string nome, decimal prezzo, string categoria, bool disponibile)
    {
        var p = await _db.Prodotti.FindAsync(id)
            ?? throw new InvalidOperationException($"Prodotto {id} non trovato.");
        p.Nome = nome;
        p.Prezzo = prezzo;
        p.Categoria = categoria;
        p.Disponibile = disponibile;
        await _db.SaveChangesAsync();
    }

    public async Task EliminaProdottoAsync(int id)
    {
        var p = await _db.Prodotti.FindAsync(id)
            ?? throw new InvalidOperationException($"Prodotto {id} non trovato.");
        _db.Prodotti.Remove(p);
        await _db.SaveChangesAsync();
    }

    public async Task RinominaCategoriaAsync(string vecchia, string nuova)
    {
        var prodotti = await _db.Prodotti.Where(p => p.Categoria == vecchia).ToListAsync();
        foreach (var p in prodotti) p.Categoria = nuova;
        await _db.SaveChangesAsync();
    }

    public async Task EliminaCategoriaAsync(string categoria)
    {
        var prodotti = await _db.Prodotti.Where(p => p.Categoria == categoria).ToListAsync();
        _db.Prodotti.RemoveRange(prodotti);
        await _db.SaveChangesAsync();
    }
}
