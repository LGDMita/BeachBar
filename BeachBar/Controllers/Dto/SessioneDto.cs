using BeachBar.Core.Entities;

namespace BeachBar.Controllers.Dto;

public class SessioneDto
{
    public int Id { get; set; }
    public int OmbrelloneId { get; set; }
    public int NumeroOmbrellone { get; set; }
    public string? NomeCliente { get; set; }
    public bool Chiusa { get; set; }
    public DateTime Apertura { get; set; }
    public DateTime? Chiusura { get; set; }
    public decimal Totale { get; set; }
    public List<ConsumazioneDto> Consumazioni { get; set; } = [];

    public static SessioneDto FromEntity(Sessione s) => new()
    {
        Id = s.Id,
        OmbrelloneId = s.OmbrelloneId,
        NumeroOmbrellone = s.Ombrellone?.Numero ?? 0,
        NomeCliente = s.NomeCliente,
        Chiusa = s.Chiusa,
        Apertura = s.Apertura,
        Chiusura = s.Chiusura,
        Totale = s.Consumazioni.Sum(c => c.Quantita * c.Prodotto.Prezzo),
        Consumazioni = s.Consumazioni.Select(ConsumazioneDto.FromEntity).ToList()
    };
}
