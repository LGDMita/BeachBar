using BeachBar.Controllers.Dto;
using BeachBar.Controllers.Dto.Request;
using BeachBar.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeachBar.Controllers;

/// <summary>
/// Gestisce le consumazioni (ordini) all'interno di una sessione.
/// La route è annidata sotto /api/sessioni/{sessioneId}/ordini per rendere
/// esplicita la relazione: un ordine appartiene sempre a una sessione specifica.
/// </summary>
[ApiController]
[Route("api/sessioni/{sessioneId:int}/ordini")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class OrdiniController : ControllerBase
{
    private readonly ISessioniService _sessioni;
    private readonly IProdottiService _prodotti;
    private readonly IConsumazioniService _consumazioni;
    private readonly ILogger<OrdiniController> _logger;

    public OrdiniController(
        ISessioniService sessioni,
        IProdottiService prodotti,
        IConsumazioniService consumazioni,
        ILogger<OrdiniController> logger)
    {
        _sessioni = sessioni;
        _prodotti = prodotti;
        _consumazioni = consumazioni;
        _logger = logger;
    }

    /// <summary>Restituisce tutte le consumazioni di una sessione.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ConsumazioneDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrdini(int sessioneId)
    {
        var sessione = await _sessioni.GetSessioneByIdAsync(sessioneId);
        if (sessione == null)
            return NotFound("Sessione non trovata");

        return Ok(sessione.Consumazioni.Select(ConsumazioneDto.FromEntity).ToList());
    }

    /// <summary>
    /// Aggiunge un prodotto alla sessione con la quantità specificata.
    /// Se il prodotto è già presente, la quantità viene sommata a quella esistente.
    /// Non è possibile aggiungere ordini a una sessione già chiusa.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ConsumazioneDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AggiungiOrdine(int sessioneId, [FromBody] AggiungiConsumazioneRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var sessione = await _sessioni.GetSessioneByIdAsync(sessioneId);
        if (sessione == null)
            return NotFound("Sessione non trovata");

        if (sessione.Chiusa)
            return Conflict("Impossibile aggiungere ordini a una sessione chiusa");

        var prodotto = await _prodotti.GetProdottoByIdAsync(request.ProdottoId);
        if (prodotto == null)
            return NotFound("Prodotto non trovato");

        var consumazione = await _consumazioni.AggiungiConsumazioneConQuantitaAsync(sessioneId, request.ProdottoId, request.Quantita);
        return StatusCode(StatusCodes.Status201Created, ConsumazioneDto.FromEntity(consumazione));
    }

    /// <summary>
    /// Rimuove una consumazione dalla sessione.
    /// Non è possibile eliminare ordini da una sessione già chiusa.
    /// </summary>
    [HttpDelete("{ordineId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> EliminaOrdine(int sessioneId, int ordineId)
    {
        var sessione = await _sessioni.GetSessioneByIdAsync(sessioneId);
        if (sessione == null)
            return NotFound("Sessione non trovata");

        if (sessione.Chiusa)
            return Conflict("Impossibile eliminare ordini da una sessione chiusa");

        var trovato = await _consumazioni.EliminaConsumazioneByIdAsync(ordineId);
        if (!trovato)
            return NotFound("Ordine non trovato");

        return NoContent();
    }
}
