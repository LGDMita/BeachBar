using System.ComponentModel.DataAnnotations;

namespace BeachBar.Controllers.Dto.Request;

public class ApriSessioneRequest
{
    [Required]
    public int OmbrelloneId { get; set; }

    public string? NomeCliente { get; set; }
}
