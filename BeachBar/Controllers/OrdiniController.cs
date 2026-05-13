using BeachBar.Controllers.Dto;
using BeachBar.Controllers.Dto.Request;
using BeachBar.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace BeachBar.Controllers;

[ApiController]
[Route("api/sessioni/{sessioneId:int}/ordini")]
public class OrdiniController : ControllerBase
{
    private readonly IBeachBarService _service;
    private readonly ILogger<OrdiniController> _logger;

    public OrdiniController(IBeachBarService service, ILogger<OrdiniController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ConsumazioneDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrdini(int sessioneId)
    {
        var sessione = await _service.GetSessioneByIdAsync(sessioneId);
        if (sessione == null)
            return NotFound("Sessione non trovata");

        return Ok(sessione.Consumazioni.Select(ConsumazioneDto.FromEntity).ToList());
    }

    [HttpPost]
    [ProducesResponseType(typeof(ConsumazioneDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AggiungiOrdine(int sessioneId, [FromBody] AggiungiConsumazioneRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var sessione = await _service.GetSessioneByIdAsync(sessioneId);
        if (sessione == null)
            return NotFound("Sessione non trovata");

        if (sessione.Chiusa)
            return Conflict("Impossibile aggiungere ordini a una sessione chiusa");

        var prodotto = await _service.GetProdottoByIdAsync(request.ProdottoId);
        if (prodotto == null)
            return NotFound("Prodotto non trovato");

        var consumazione = await _service.AggiungiConsumazioneConQuantitaAsync(sessioneId, request.ProdottoId, request.Quantita);
        return StatusCode(StatusCodes.Status201Created, ConsumazioneDto.FromEntity(consumazione));
    }

    [HttpDelete("{ordineId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> EliminaOrdine(int sessioneId, int ordineId)
    {
        var sessione = await _service.GetSessioneByIdAsync(sessioneId);
        if (sessione == null)
            return NotFound("Sessione non trovata");

        if (sessione.Chiusa)
            return Conflict("Impossibile eliminare ordini da una sessione chiusa");

        var trovato = await _service.EliminaConsumazioneByIdAsync(ordineId);
        if (!trovato)
            return NotFound("Ordine non trovato");

        return NoContent();
    }
}
