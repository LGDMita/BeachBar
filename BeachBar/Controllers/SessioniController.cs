using BeachBar.Controllers.Dto;
using BeachBar.Controllers.Dto.Request;
using BeachBar.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeachBar.Controllers;

/// <summary>
/// Gestisce il ciclo di vita delle sessioni: apertura, consultazione e chiusura.
/// Una sessione rappresenta il "conto aperto" di un cliente su un ombrellone.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class SessioniController : ControllerBase
{
    private readonly ISessioniService _sessioni;
    private readonly ILogger<SessioniController> _logger;

    public SessioniController(ISessioniService sessioni, ILogger<SessioniController> logger)
    {
        _sessioni = sessioni;
        _logger = logger;
    }

    /// <summary>Restituisce tutte le sessioni (aperte e chiuse).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<SessioneDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSessioni()
    {
        try
        {
            var sessioni = await _sessioni.GetTutteSessioniAsync();
            return Ok(sessioni.Select(SessioneDto.FromEntity).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero delle sessioni");
            return StatusCode(500, "Errore interno del server.");
        }
    }

    /// <summary>Restituisce solo le sessioni attualmente aperte.</summary>
    [HttpGet("aperte")]
    [ProducesResponseType(typeof(List<SessioneDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSessioniAperte()
    {
        try
        {
            var sessioni = await _sessioni.GetSessioniAperteAsync();
            return Ok(sessioni.Select(SessioneDto.FromEntity).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero delle sessioni aperte");
            return StatusCode(500, "Errore interno del server.");
        }
    }

    /// <summary>Restituisce una singola sessione con l'elenco completo delle consumazioni.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SessioneDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSessione(int id)
    {
        try
        {
            var sessione = await _sessioni.GetSessioneByIdAsync(id);
            if (sessione == null)
                return NotFound("Sessione non trovata");

            return Ok(SessioneDto.FromEntity(sessione));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero della sessione {Id}", id);
            return StatusCode(500, "Errore interno del server.");
        }
    }

    /// <summary>
    /// Apre una nuova sessione per un ombrellone.
    /// Verifica che l'ombrellone esista e che non abbia già una sessione aperta.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SessioneDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ApriSessione([FromBody] ApriSessioneRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var ombrellone = await _sessioni.GetOmbrelloneByIdAsync(request.OmbrelloneId);
            if (ombrellone == null)
                return NotFound("Ombrellone non trovato");

            var sessioneEsistente = await _sessioni.GetSessioneAttivaAsync(request.OmbrelloneId);
            if (sessioneEsistente != null)
                return Conflict("Esiste già una sessione aperta per questo ombrellone");

            await _sessioni.ApriSessioneAsync(request.OmbrelloneId, request.NomeCliente);

            // Il servizio non restituisce la sessione appena creata,
            // quindi la recuperiamo subito dopo con una seconda query.
            var nuovaSessione = await _sessioni.GetSessioneAttivaAsync(request.OmbrelloneId);
            var dto = SessioneDto.FromEntity(nuovaSessione!);
            return CreatedAtAction(nameof(GetSessione), new { id = dto.Id }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nell'apertura della sessione per ombrellone {Id}", request.OmbrelloneId);
            return StatusCode(500, "Errore interno del server.");
        }
    }

    /// <summary>
    /// Chiude una sessione e calcola il totale finale.
    /// Una sessione già chiusa non può essere chiusa di nuovo (409 Conflict).
    /// </summary>
    [HttpPut("{id:int}/chiudi")]
    [ProducesResponseType(typeof(SessioneDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChiudiSessione(int id)
    {
        try
        {
            var sessione = await _sessioni.GetSessioneByIdAsync(id);
            if (sessione == null)
                return NotFound("Sessione non trovata");

            if (sessione.Chiusa)
                return Conflict("La sessione è già chiusa");

            await _sessioni.ChiudiSessioneAsync(id);

            // Rileggiamo dopo la chiusura per includere data di chiusura e totale aggiornato.
            var sessioneChiusa = await _sessioni.GetSessioneByIdAsync(id);
            return Ok(SessioneDto.FromEntity(sessioneChiusa!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nella chiusura della sessione {Id}", id);
            return StatusCode(500, "Errore interno del server.");
        }
    }
}
