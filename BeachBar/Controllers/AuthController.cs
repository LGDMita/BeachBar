using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BeachBar.Controllers.Dto;
using BeachBar.Controllers.Dto.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using BC = BCrypt.Net.BCrypt;

namespace BeachBar.Controllers;

/// <summary>
/// Endpoint pubblico per l'autenticazione.
/// POST /api/auth/login: riceve username e password, restituisce un JWT Bearer
/// da usare nell'header Authorization di tutte le chiamate successive.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Valida le credenziali e restituisce un JWT.
    /// Le credenziali (username + BCrypt hash della password) sono in appsettings.json.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var expectedUsername = _config["Admin:Username"] ?? "";
        var expectedHash     = _config["Admin:PasswordHash"] ?? "";

        var usernameOk = string.Equals(request.Username, expectedUsername, StringComparison.OrdinalIgnoreCase);
        var passwordOk = usernameOk && BC.Verify(request.Password, expectedHash);

        if (!passwordOk)
            return Unauthorized("Credenziali non valide.");

        var token = BuildJwt();
        return Ok(token);
    }

    private TokenDto BuildJwt()
    {
        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var minutes = int.Parse(_config["Jwt:ExpiresMinutes"] ?? "480");
        var expires = DateTime.UtcNow.AddMinutes(minutes);

        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Issuer"],
            audience:           _config["Jwt:Audience"],
            claims:             [new Claim(ClaimTypes.Name, _config["Admin:Username"]!)],
            notBefore:          DateTime.UtcNow,
            expires:            expires,
            signingCredentials: creds);

        return new TokenDto
        {
            Token     = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expires
        };
    }
}
