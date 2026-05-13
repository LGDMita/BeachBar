using BeachBar.Controllers.Dto;
using BeachBar.Controllers.Dto.Request;
using BeachBar.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace BeachBar.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessioniController : ControllerBase
{
    private readonly IBeachBarService _service;
    private readonly ILogger<SessioniController> _logger;

    public SessioniController(IBeachBarService service, ILogger<SessioniController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<SessioneDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSessioni()
    {
        var sessioni = await _service.GetTutteSessioniAsync();
        return Ok(sessioni.Select(SessioneDto.FromEntity).ToList());
    }

    [HttpGet("aperte")]
    [ProducesResponseType(typeof(List<SessioneDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSessioniAperte()
    {
        var sessioni = await _service.GetSessioniAperteAsync();
        return Ok(sessioni.Select(SessioneDto.FromEntity).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SessioneDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSessione(int id)
    {
        var sessione = await _service.GetSessioneByIdAsync(id);
        if (sessione == null)
            return NotFound("Sessione non trovata");

        return Ok(SessioneDto.FromEntity(sessione));
    }

    [HttpPost]
    [ProducesResponseType(typeof(SessioneDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ApriSessione([FromBody] ApriSessioneRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ombrellone = await _service.GetOmbrelloneByIdAsync(request.OmbrelloneId);
        if (ombrellone == null)
            return NotFound("Ombrellone non trovato");

        var sessioneEsistente = await _service.GetSessioneAttivaAsync(request.OmbrelloneId);
        if (sessioneEsistente != null)
            return Conflict("Esiste già una sessione aperta per questo ombrellone");

        await _service.ApriSessioneAsync(request.OmbrelloneId, request.NomeCliente);

        var nuovaSessione = await _service.GetSessioneAttivaAsync(request.OmbrelloneId);
        var dto = SessioneDto.FromEntity(nuovaSessione!);
        return CreatedAtAction(nameof(GetSessione), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:int}/chiudi")]
    [ProducesResponseType(typeof(SessioneDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChiudiSessione(int id)
    {
        var sessione = await _service.GetSessioneByIdAsync(id);
        if (sessione == null)
            return NotFound("Sessione non trovata");

        if (sessione.Chiusa)
            return Conflict("La sessione è già chiusa");

        await _service.ChiudiSessioneAsync(id);

        var sessioneChiusa = await _service.GetSessioneByIdAsync(id);
        return Ok(SessioneDto.FromEntity(sessioneChiusa!));
    }
}
