namespace Bison.Razor.domain_model;
public class Proposal : Post
{
    public Observation Observation { get; set; } = null!;

    public Taxon Taxon { get; set; } = null!;
}