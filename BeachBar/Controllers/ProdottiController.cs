using BeachBar.Controllers.Dto;
using BeachBar.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace BeachBar.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdottiController : ControllerBase
{
    private readonly IBeachBarService _service;
    private readonly ILogger<ProdottiController> _logger;

    public ProdottiController(IBeachBarService service, ILogger<ProdottiController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ProdottoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProdotti()
    {
        var prodotti = await _service.GetTuttiProdottiAsync();
        return Ok(prodotti.Select(ProdottoDto.FromEntity).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProdottoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProdotto(int id)
    {
        var prodotto = await _service.GetProdottoByIdAsync(id);
        if (prodotto == null)
            return NotFound("Prodotto non trovato");

        return Ok(ProdottoDto.FromEntity(prodotto));
    }

    [HttpGet("categoria/{categoria}")]
    [ProducesResponseType(typeof(List<ProdottoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProdottiPerCategoria(string categoria)
    {
        var prodotti = await _service.GetProdottiPerCategoriaAsync(categoria);
        return Ok(prodotti.Select(ProdottoDto.FromEntity).ToList());
    }
}
