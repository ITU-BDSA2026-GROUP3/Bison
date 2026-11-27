namespace Bison.infrastructure.DTOs;

public class CommentDto
{
    public int obsId{ get; set; }
    public string Text { get; set; } = "";

    public string Timestamp { get; set; } = "";

    public string AuthorId { get; set; } = "";
    public string AuthorName { get; set; } = "";

}