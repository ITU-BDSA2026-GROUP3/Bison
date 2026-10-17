namespace Bison.Razor.DTOs;

public class ObservationDto
{
    public int Id { get; set; }

    public string Text { get; set; } = "";

    public string Timestamp { get; set; } = "";

    public string AuthorName { get; set; } = "";

    public string TaxonName { get; set; } = "";
}