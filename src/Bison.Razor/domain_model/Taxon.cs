using SQLitePCL;
namespace Bison.Razor.domain_model;
public class Taxon
{
    public int Id { get; set; }

    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    
    public Taxon? ancestor {get; set; }

    public List<Observation> Observations { get; set; } = new();
    public List<Proposal> Proposals { get; set; } = new();

    public bool isSubTaxon(Taxon Ancestor)
    {
        if(Ancestor == ancestor) 
        return true;
        else return false;
    }
}