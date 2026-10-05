namespace squad_api.DTOs;

public class FormationDto
{
    public string Name { get; set; } = string.Empty;
    public int Defence { get; set; }
    public int Midfield { get; set; }
    public int Attack { get; set; }
}

public record FormationResponse(int Id, string Name, int Defence, int Midfield, int Attack)
{
    public static FormationResponse From(Formation f) => new(f.Id, f.Name, f.Defence, f.Midfield, f.Attack);
}
