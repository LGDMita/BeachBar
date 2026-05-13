using BeachBar.Core.Entities;

namespace BeachBar.Controllers.Dto;

public class ConsumazioneDto
{
    public int Id { get; set; }
    public int ProdottoId { get; set; }
    public string NomeProdotto { get; set; } = string.Empty;
    public int Quantita { get; set; }
    public decimal PrezzoUnitario { get; set; }
    public decimal Subtotale { get; set; }

    public static ConsumazioneDto FromEntity(Consumazione c) => new()
    {
        Id = c.Id,
        ProdottoId = c.ProdottoId,
        NomeProdotto = c.Prodotto.Nome,
        Quantita = c.Quantita,
        PrezzoUnitario = c.Prodotto.Prezzo,
        Subtotale = c.Quantita * c.Prodotto.Prezzo
    };
}
