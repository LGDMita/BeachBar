using BeachBar.Core.Entities;

namespace BeachBar.Controllers.Dto;

public class ProdottoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Prezzo { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public bool Disponibile { get; set; }

    public static ProdottoDto FromEntity(Prodotto p) => new()
    {
        Id = p.Id,
        Nome = p.Nome,
        Prezzo = p.Prezzo,
        Categoria = p.Categoria,
        Disponibile = p.Disponibile
    };
}
