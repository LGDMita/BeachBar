namespace BeachBar.Core.Entities;

public class ImpostazioniSpiaggia
{
    public int Id { get; set; }
    public int NumeroOmbrelloni { get; set; }
    public int NumeroColonne { get; set; }
    public int NumeroRighe { get; set; } = 5;
    public string? BordiVerticali { get; set; }
    public string? BordiOrizzontali { get; set; }
    public DateTime? UltimoResetStatistiche { get; set; }
}