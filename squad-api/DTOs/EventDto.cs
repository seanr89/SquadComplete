namespace squad_api.DTOs;

public class EventDto
{
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string Level { get; set; } = null!;
}

public record EventResponse(int Id, string Title, string Message, string Level, DateTime CreatedAt)
{
    public static EventResponse From(Event e) => new(e.Id, e.Title, e.Message, e.Level, e.CreatedAt);
}
