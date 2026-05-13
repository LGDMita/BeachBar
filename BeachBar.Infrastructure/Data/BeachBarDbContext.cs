using BeachBar.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeachBar.Infrastructure.Data;

public class BeachBarDbContext : DbContext
{
    public BeachBarDbContext(DbContextOptions<BeachBarDbContext> options) : base(options) { }

    public DbSet<Ombrellone> Ombrelloni { get; set; }
    public DbSet<Sessione> Sessioni { get; set; }
    public DbSet<Prodotto> Prodotti { get; set; }
    public DbSet<Consumazione> Consumazioni { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Seed ombrelloni
        var ombrelloni = Enumerable.Range(1, 20)
            .Select(i => new Ombrellone { Id = i, Numero = i, Occupato = false })
            .ToList();
        modelBuilder.Entity<Ombrellone>().HasData(ombrelloni);

        // Seed prodotti
        modelBuilder.Entity<Prodotto>().HasData(
            new Prodotto { Id = 1, Nome = "Acqua", Prezzo = 1.50m, Disponibile = true, Categoria = "Bibite" },
            new Prodotto { Id = 2, Nome = "Coca Cola", Prezzo = 2.50m, Disponibile = true, Categoria = "Bibite" },
            new Prodotto { Id = 3, Nome = "Succo di frutta", Prezzo = 2.50m, Disponibile = true, Categoria = "Bibite" },
            new Prodotto { Id = 4, Nome = "Birra", Prezzo = 3.00m, Disponibile = true, Categoria = "Alcolici" },
            new Prodotto { Id = 5, Nome = "Spritz", Prezzo = 4.00m, Disponibile = true, Categoria = "Alcolici" },
            new Prodotto { Id = 6, Nome = "Vino bianco", Prezzo = 3.50m, Disponibile = true, Categoria = "Alcolici" },
            new Prodotto { Id = 7, Nome = "Caffè", Prezzo = 1.20m, Disponibile = true, Categoria = "Caffetteria" },
            new Prodotto { Id = 8, Nome = "Cappuccino", Prezzo = 1.50m, Disponibile = true, Categoria = "Caffetteria" },
            new Prodotto { Id = 9, Nome = "Tramezzino", Prezzo = 2.50m, Disponibile = true, Categoria = "Cibo" },
            new Prodotto { Id = 10, Nome = "Piadina", Prezzo = 4.50m, Disponibile = true, Categoria = "Cibo" },
            new Prodotto { Id = 11, Nome = "Gelato", Prezzo = 2.50m, Disponibile = true, Categoria = "Cibo" }
        );
    }
}