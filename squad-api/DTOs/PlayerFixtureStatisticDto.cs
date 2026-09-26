namespace squad_api.DTOs;

public class PlayerFixtureStatisticDto
{
    public int FixtureId { get; set; }
    public int PlayerId { get; set; }
    public int? TeamId { get; set; }
    public int? Minutes { get; set; }
    public int? Number { get; set; }
    public string? Position { get; set; }
    public decimal? Rating { get; set; }
    public bool IsCaptain { get; set; }
    public bool IsSubstitute { get; set; }
}
