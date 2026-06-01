using BeachBar.Controllers.Dto;
using BeachBar.Controllers.Dto.Request;
using BeachBar.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeachBar.Controllers;

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

    /// <summary>Apre una sessione su un ombrellone. Consente più sessioni contemporanee sullo stesso ombrellone.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(SessioneDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

            var data = request.DataRiferimento ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var nuovaSessione = await _sessioni.ApriSessioneAsync(request.OmbrelloneId, request.NomeCliente, data);
            return CreatedAtAction(nameof(GetSessione), new { id = nuovaSessione.Id }, SessioneDto.FromEntity(nuovaSessione));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nell'apertura della sessione per ombrellone {Id}", request.OmbrelloneId);
            return StatusCode(500, "Errore interno del server.");
        }
    }

    /// <summary>Apre un conto extra senza ombrellone (ospite volante).</summary>
    [HttpPost("extra")]
    [ProducesResponseType(typeof(SessioneDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ApriContoExtra([FromBody] ApriContoExtraRequest request)
    {
        try
        {
            var data = request.DataRiferimento ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var nuova = await _sessioni.ApriContoExtraAsync(request.NomeCliente, data);
            return CreatedAtAction(nameof(GetSessione), new { id = nuova.Id }, SessioneDto.FromEntity(nuova));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nell'apertura del conto extra");
            return StatusCode(500, "Errore interno del server.");
        }
    }

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

            await _sessioni.ChiudiSessioneAsync(id, liberaOmbrellone: true);

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
