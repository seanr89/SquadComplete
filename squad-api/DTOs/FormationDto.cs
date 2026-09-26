namespace squad_api.DTOs;

public class FormationDto
{
    public string Name { get; set; } = string.Empty;
    public int Defence { get; set; }
    public int Midfield { get; set; }
    public int Attack { get; set; }
}
