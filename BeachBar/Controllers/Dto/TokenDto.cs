namespace BeachBar.Controllers.Dto;

public class TokenDto
{
    public string Token { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
}
