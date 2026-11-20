namespace Bison.Razor.domain_model;
public class Observation : Post
{
    public byte[]? ImgBytes { get; set; }

    public string? Licence { get; set; }

    public Taxon Taxon { get; set; } = null!;

    public List<Comment> Comments { get; set; } = new();
    public List<Proposal> Proposals { get; set; } = new();
    public Taxon getTaxon()
    {
        return Taxon;
    }
}