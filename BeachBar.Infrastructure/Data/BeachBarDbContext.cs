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
    public DbSet<ImpostazioniSpiaggia> ImpostazioniSpiaggia { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Seed ombrelloni
        var ombrelloni = Enumerable.Range(1, 20)
            .Select(i => new Ombrellone
            {
                Id = i,
                Numero = i,
                Occupato = false
            })
            .ToList();

        modelBuilder.Entity<Ombrellone>().HasData(ombrelloni);

        // Seed prodotti
        modelBuilder.Entity<Prodotto>().HasData(

            // PANINI / PIADINE / FOCACCIA
            new Prodotto { Id = 1, Nome = "Tempesta", Prezzo = 7.00m, Disponibile = true, Categoria = "Panini" },
            new Prodotto { Id = 2, Nome = "Scirocco", Prezzo = 7.00m, Disponibile = true, Categoria = "Panini" },
            new Prodotto { Id = 3, Nome = "Maestrale", Prezzo = 7.00m, Disponibile = true, Categoria = "Panini" },
            new Prodotto { Id = 4, Nome = "Ponente", Prezzo = 7.00m, Disponibile = true, Categoria = "Panini" },
            new Prodotto { Id = 5, Nome = "Hot Dog", Prezzo = 5.00m, Disponibile = true, Categoria = "Panini" },
            new Prodotto { Id = 6, Nome = "Maxi Toast", Prezzo = 6.00m, Disponibile = true, Categoria = "Panini" },
            new Prodotto { Id = 7, Nome = "Maxi Toast Grecale", Prezzo = 7.00m, Disponibile = true, Categoria = "Panini" },
            new Prodotto { Id = 8, Nome = "Hamburger", Prezzo = 9.00m, Disponibile = true, Categoria = "Panini" },
            new Prodotto { Id = 9, Nome = "Cheeseburger", Prezzo = 10.00m, Disponibile = true, Categoria = "Panini" },

            // INSALATE
            new Prodotto { Id = 10, Nome = "Insalata", Prezzo = 12.00m, Disponibile = true, Categoria = "Insalate" },
            new Prodotto { Id = 11, Nome = "Condiglione", Prezzo = 12.00m, Disponibile = true, Categoria = "Insalate" },
            new Prodotto { Id = 12, Nome = "Nizzarda", Prezzo = 12.00m, Disponibile = true, Categoria = "Insalate" },
            new Prodotto { Id = 13, Nome = "Ultima Spiaggia", Prezzo = 14.00m, Disponibile = true, Categoria = "Insalate" },

            // PIZZA
            new Prodotto { Id = 14, Nome = "Pizzella Margherita", Prezzo = 6.00m, Disponibile = true, Categoria = "Pizza" },
            new Prodotto { Id = 15, Nome = "Pizzella Farcita", Prezzo = 8.00m, Disponibile = true, Categoria = "Pizza" },

            // PIATTI
            new Prodotto { Id = 16, Nome = "Caprese", Prezzo = 10.00m, Disponibile = true, Categoria = "Piatti" },
            new Prodotto { Id = 17, Nome = "Crudo e Melone", Prezzo = 12.00m, Disponibile = true, Categoria = "Piatti" },
            new Prodotto { Id = 18, Nome = "Bresaola Rucola Grana Pomodoro", Prezzo = 15.00m, Disponibile = true, Categoria = "Piatti" },
            new Prodotto { Id = 19, Nome = "Crudo Bufala Rucola", Prezzo = 15.00m, Disponibile = true, Categoria = "Piatti" },
            new Prodotto { Id = 20, Nome = "Patatine Fritte", Prezzo = 5.00m, Disponibile = true, Categoria = "Piatti" },
            new Prodotto { Id = 21, Nome = "Pan Fritto", Prezzo = 5.00m, Disponibile = true, Categoria = "Piatti" },
            new Prodotto { Id = 22, Nome = "Nuggets e Patatine", Prezzo = 10.00m, Disponibile = true, Categoria = "Piatti" },
            new Prodotto { Id = 23, Nome = "Pan Fritto con Crudo", Prezzo = 10.00m, Disponibile = true, Categoria = "Piatti" },
            new Prodotto { Id = 24, Nome = "Wurstel e Patatine", Prezzo = 10.00m, Disponibile = true, Categoria = "Piatti" },
            new Prodotto { Id = 25, Nome = "Cotoletta e Patatine", Prezzo = 10.00m, Disponibile = true, Categoria = "Piatti" },
            new Prodotto { Id = 26, Nome = "Torta di Verdure", Prezzo = 5.00m, Disponibile = true, Categoria = "Piatti" },
            new Prodotto { Id = 27, Nome = "Torta di Verdure con Contorno", Prezzo = 10.00m, Disponibile = true, Categoria = "Piatti" },
            new Prodotto { Id = 28, Nome = "Focaccia al Formaggio", Prezzo = 12.00m, Disponibile = true, Categoria = "Piatti" },
            new Prodotto { Id = 29, Nome = "Focaccia al Formaggio con Pomodoro", Prezzo = 14.00m, Disponibile = true, Categoria = "Piatti" },

            // PASTA
            new Prodotto { Id = 30, Nome = "Penne", Prezzo = 8.00m, Disponibile = true, Categoria = "Pasta" },
            new Prodotto { Id = 31, Nome = "Gnocchi", Prezzo = 8.00m, Disponibile = true, Categoria = "Pasta" },
            new Prodotto { Id = 32, Nome = "Trofie", Prezzo = 8.00m, Disponibile = true, Categoria = "Pasta" },
            new Prodotto { Id = 33, Nome = "Ravioli", Prezzo = 8.00m, Disponibile = true, Categoria = "Pasta" },
            new Prodotto { Id = 34, Nome = "Lasagne", Prezzo = 10.00m, Disponibile = true, Categoria = "Pasta" },

            // FRUTTA
            new Prodotto { Id = 35, Nome = "Anguria", Prezzo = 4.00m, Disponibile = true, Categoria = "Frutta" },
            new Prodotto { Id = 36, Nome = "Melone", Prezzo = 4.00m, Disponibile = true, Categoria = "Frutta" },
            new Prodotto { Id = 37, Nome = "Macedonia", Prezzo = 5.00m, Disponibile = true, Categoria = "Frutta" },
            new Prodotto { Id = 38, Nome = "Macedonia con Yogurt/Gelato", Prezzo = 7.00m, Disponibile = true, Categoria = "Frutta" },

            // CAFFETTERIA
            new Prodotto { Id = 39, Nome = "Caffè", Prezzo = 1.30m, Disponibile = true, Categoria = "Caffetteria" },
            new Prodotto { Id = 40, Nome = "Caffè Americano", Prezzo = 1.50m, Disponibile = true, Categoria = "Caffetteria" },
            new Prodotto { Id = 41, Nome = "Caffè Decaffeinato", Prezzo = 1.50m, Disponibile = true, Categoria = "Caffetteria" },
            new Prodotto { Id = 42, Nome = "Orzo/Ginseng", Prezzo = 1.50m, Disponibile = true, Categoria = "Caffetteria" },
            new Prodotto { Id = 43, Nome = "Orzo/Ginseng Grande", Prezzo = 2.00m, Disponibile = true, Categoria = "Caffetteria" },
            new Prodotto { Id = 44, Nome = "Caffè Corretto", Prezzo = 1.80m, Disponibile = true, Categoria = "Caffetteria" },
            new Prodotto { Id = 45, Nome = "Caffè Shakerato", Prezzo = 2.50m, Disponibile = true, Categoria = "Caffetteria" },
            new Prodotto { Id = 46, Nome = "Caffè Shakerato Special", Prezzo = 4.00m, Disponibile = true, Categoria = "Caffetteria" },
            new Prodotto { Id = 47, Nome = "Cappuccino", Prezzo = 2.00m, Disponibile = true, Categoria = "Caffetteria" },
            new Prodotto { Id = 48, Nome = "Latte", Prezzo = 1.50m, Disponibile = true, Categoria = "Caffetteria" },
            new Prodotto { Id = 49, Nome = "Latte Macchiato", Prezzo = 2.50m, Disponibile = true, Categoria = "Caffetteria" },
            new Prodotto { Id = 50, Nome = "Brioches", Prezzo = 1.50m, Disponibile = true, Categoria = "Caffetteria" },
            new Prodotto { Id = 51, Nome = "Focaccia", Prezzo = 2.00m, Disponibile = true, Categoria = "Caffetteria" },
            new Prodotto { Id = 52, Nome = "Pizza", Prezzo = 2.50m, Disponibile = true, Categoria = "Caffetteria" },
            new Prodotto { Id = 53, Nome = "Crema Caffè", Prezzo = 3.00m, Disponibile = true, Categoria = "Caffetteria" },
            new Prodotto { Id = 54, Nome = "Yogurt", Prezzo = 3.00m, Disponibile = true, Categoria = "Caffetteria" },

            // BIBITE
            new Prodotto { Id = 55, Nome = "Acqua 0.5L", Prezzo = 1.50m, Disponibile = true, Categoria = "Bibite" },
            new Prodotto { Id = 56, Nome = "Acqua 1L", Prezzo = 2.50m, Disponibile = true, Categoria = "Bibite" },
            new Prodotto { Id = 57, Nome = "Estathé Brick", Prezzo = 2.00m, Disponibile = true, Categoria = "Bibite" },
            new Prodotto { Id = 58, Nome = "Succhi di Frutta", Prezzo = 3.00m, Disponibile = true, Categoria = "Bibite" },
            new Prodotto { Id = 59, Nome = "Tonica Schweppes", Prezzo = 3.00m, Disponibile = true, Categoria = "Bibite" },
            new Prodotto { Id = 60, Nome = "Lemonsoda", Prezzo = 3.00m, Disponibile = true, Categoria = "Bibite" },
            new Prodotto { Id = 61, Nome = "Chinotto di Lurisia", Prezzo = 4.00m, Disponibile = true, Categoria = "Bibite" },
            new Prodotto { Id = 62, Nome = "Limonata del Tigullio", Prezzo = 4.00m, Disponibile = true, Categoria = "Bibite" },
            new Prodotto { Id = 63, Nome = "Aloe Vera", Prezzo = 5.00m, Disponibile = true, Categoria = "Bibite" },
            new Prodotto { Id = 64, Nome = "Lattine", Prezzo = 3.00m, Disponibile = true, Categoria = "Bibite" },
            new Prodotto { Id = 65, Nome = "Spremuta di Arancia", Prezzo = 4.00m, Disponibile = true, Categoria = "Bibite" },
            new Prodotto { Id = 66, Nome = "Smoothie di Frutta", Prezzo = 6.00m, Disponibile = true, Categoria = "Bibite" },

            // BIRRE
            new Prodotto { Id = 67, Nome = "Birra Raffo Piccola", Prezzo = 3.50m, Disponibile = true, Categoria = "Birre" },
            new Prodotto { Id = 68, Nome = "Birra Raffo Media", Prezzo = 6.00m, Disponibile = true, Categoria = "Birre" },
            new Prodotto { Id = 69, Nome = "Birra Raffo Maxi", Prezzo = 7.00m, Disponibile = true, Categoria = "Birre" },
            new Prodotto { Id = 70, Nome = "Birra Raffo Ultra", Prezzo = 12.00m, Disponibile = true, Categoria = "Birre" },
            new Prodotto { Id = 71, Nome = "Heineken", Prezzo = 4.50m, Disponibile = true, Categoria = "Birre" },
            new Prodotto { Id = 72, Nome = "Beck's", Prezzo = 4.50m, Disponibile = true, Categoria = "Birre" },
            new Prodotto { Id = 73, Nome = "Raffo Grezza", Prezzo = 4.50m, Disponibile = true, Categoria = "Birre" },
            new Prodotto { Id = 74, Nome = "Nastro Azzurro Capri", Prezzo = 4.50m, Disponibile = true, Categoria = "Birre" },
            new Prodotto { Id = 75, Nome = "Ceres", Prezzo = 5.00m, Disponibile = true, Categoria = "Birre" },
            new Prodotto { Id = 76, Nome = "Nastro Azzurro 0", Prezzo = 4.50m, Disponibile = true, Categoria = "Birre" },

            // VINI
            new Prodotto { Id = 77, Nome = "Calice Pigato/Vermentino", Prezzo = 5.00m, Disponibile = true, Categoria = "Vini" },
            new Prodotto { Id = 78, Nome = "Calice Prosecco", Prezzo = 6.00m, Disponibile = true, Categoria = "Vini" },
            new Prodotto { Id = 79, Nome = "Pigato Le Creuze", Prezzo = 22.00m, Disponibile = true, Categoria = "Vini" },
            new Prodotto { Id = 80, Nome = "Vermentino Le Creuze", Prezzo = 22.00m, Disponibile = true, Categoria = "Vini" },
            new Prodotto { Id = 81, Nome = "Prosecco Marsuret", Prezzo = 25.00m, Disponibile = true, Categoria = "Vini" },
            new Prodotto { Id = 82, Nome = "Franciacorta Ca del Bosco", Prezzo = 55.00m, Disponibile = true, Categoria = "Vini" },

            // APERITIVI
            new Prodotto { Id = 83, Nome = "Crodino", Prezzo = 3.50m, Disponibile = true, Categoria = "Aperitivi" },
            new Prodotto { Id = 84, Nome = "Sanbitter", Prezzo = 3.50m, Disponibile = true, Categoria = "Aperitivi" },
            new Prodotto { Id = 85, Nome = "Campari Soda", Prezzo = 4.00m, Disponibile = true, Categoria = "Aperitivi" },
            new Prodotto { Id = 86, Nome = "Aperol Spritz", Prezzo = 7.00m, Disponibile = true, Categoria = "Aperitivi" },
            new Prodotto { Id = 87, Nome = "Campari Spritz", Prezzo = 8.00m, Disponibile = true, Categoria = "Aperitivi" },
            new Prodotto { Id = 88, Nome = "Hugo", Prezzo = 8.00m, Disponibile = true, Categoria = "Aperitivi" },
            new Prodotto { Id = 89, Nome = "Sprutz", Prezzo = 8.00m, Disponibile = true, Categoria = "Aperitivi" },
            new Prodotto { Id = 90, Nome = "Negroni", Prezzo = 8.00m, Disponibile = true, Categoria = "Aperitivi" },
            new Prodotto { Id = 91, Nome = "Americano", Prezzo = 8.00m, Disponibile = true, Categoria = "Aperitivi" },
            new Prodotto { Id = 92, Nome = "Negroni Sbagliato", Prezzo = 8.00m, Disponibile = true, Categoria = "Aperitivi" },
            new Prodotto { Id = 93, Nome = "Mojito", Prezzo = 8.00m, Disponibile = true, Categoria = "Aperitivi" },
            new Prodotto { Id = 94, Nome = "Gin Tonic", Prezzo = 8.00m, Disponibile = true, Categoria = "Aperitivi" },
            new Prodotto { Id = 95, Nome = "Gin Lemon", Prezzo = 8.00m, Disponibile = true, Categoria = "Aperitivi" },
            new Prodotto { Id = 96, Nome = "Vodka Tonic", Prezzo = 8.00m, Disponibile = true, Categoria = "Aperitivi" },
            new Prodotto { Id = 97, Nome = "Vodka Lemon", Prezzo = 8.00m, Disponibile = true, Categoria = "Aperitivi" },
            new Prodotto { Id = 98, Nome = "Campari Shakerato", Prezzo = 8.00m, Disponibile = true, Categoria = "Aperitivi" }
        );

        // Seed impostazioni spiaggia
        modelBuilder.Entity<ImpostazioniSpiaggia>().HasData(
            new ImpostazioniSpiaggia
            {
                Id = 1,
                NumeroOmbrelloni = 20,
                NumeroColonne = 4,
                NumeroRighe = 5
            }
        );
    }
}