using BeachBar.Controllers.Dto;
using BeachBar.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace BeachBar.Controllers;

/// <summary>
/// Espone il listino prodotti dello stabilimento.
/// La gestione (aggiunta, modifica, eliminazione) avviene dal pannello Blazor.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProdottiController : ControllerBase
{
    private readonly IProdottiService _prodotti;
    private readonly ILogger<ProdottiController> _logger;

    public ProdottiController(IProdottiService prodotti, ILogger<ProdottiController> logger)
    {
        _prodotti = prodotti;
        _logger = logger;
    }

    /// <summary>Restituisce tutti i prodotti ordinati per categoria e nome.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProdottoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProdotti()
    {
        var prodotti = await _prodotti.GetTuttiProdottiAsync();
        return Ok(prodotti.Select(ProdottoDto.FromEntity).ToList());
    }

    /// <summary>Restituisce un singolo prodotto per ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProdottoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProdotto(int id)
    {
        var prodotto = await _prodotti.GetProdottoByIdAsync(id);
        if (prodotto == null)
            return NotFound("Prodotto non trovato");

        return Ok(ProdottoDto.FromEntity(prodotto));
    }

    /// <summary>
    /// Restituisce i prodotti filtrati per categoria.
    /// Restituisce lista vuota se la categoria non esiste: non è un errore.
    /// </summary>
    [HttpGet("categoria/{categoria}")]
    [ProducesResponseType(typeof(List<ProdottoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProdottiPerCategoria(string categoria)
    {
        var prodotti = await _prodotti.GetProdottiPerCategoriaAsync(categoria);
        return Ok(prodotti.Select(ProdottoDto.FromEntity).ToList());
    }
}
