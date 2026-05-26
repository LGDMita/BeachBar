using System.ComponentModel.DataAnnotations;

namespace BeachBar.Controllers.Dto.Request;

public class ApriSessioneRequest
{
    [Required]
    public int OmbrelloneId { get; set; }

    public string? NomeCliente { get; set; }

    /// <summary>Data di riferimento della sessione. Se omessa, usa la data odierna.</summary>
    public DateOnly? DataRiferimento { get; set; }
}
