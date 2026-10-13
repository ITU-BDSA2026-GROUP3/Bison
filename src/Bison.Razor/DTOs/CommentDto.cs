namespace Bison.Razor.DTOs;

public class CommentDto
{
    public int Id { get; set; }

    public string Text { get; set; } = "";

    public string Timestamp { get; set; } = "";

    public string AuthorName { get; set; } = "";

    public int ObservationId { get; set; }
}