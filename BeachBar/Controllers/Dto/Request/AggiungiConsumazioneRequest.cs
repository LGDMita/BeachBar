using System.ComponentModel.DataAnnotations;

namespace BeachBar.Controllers.Dto.Request;

public class AggiungiConsumazioneRequest
{
    [Required]
    public int ProdottoId { get; set; }

    [Required]
    [Range(1, 99)]
    public int Quantita { get; set; }
}
