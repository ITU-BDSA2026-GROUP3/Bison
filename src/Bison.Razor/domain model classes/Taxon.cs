public class Taxon
{
    public int Id { get; set; }

    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    public List<Observation> Observations { get; set; } = new();
    public List<Proposal> Proposals { get; set; } = new();
}