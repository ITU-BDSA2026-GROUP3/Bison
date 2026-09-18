namespace Bison.Taxonomy;

public sealed class TaxonomyRepository : ITaxonomyRepository
{
    private readonly Dictionary<string, Taxon> taxaById;
    private readonly Dictionary<string, Taxon> taxaByVernacularName;
    private readonly Dictionary<string, List<Taxon>> subtaxaByParentId;

    public TaxonomyRepository(IEnumerable<Taxon> taxa)
    {
        var taxonList = taxa.ToList();

        taxaById = taxonList
            .Where(taxon => !string.IsNullOrWhiteSpace(taxon.TaxonId))
            .ToDictionary(
                taxon => taxon.TaxonId,
                taxon => taxon,
                StringComparer.OrdinalIgnoreCase);

        taxaByVernacularName = taxonList
            .Where(taxon => !string.IsNullOrWhiteSpace(taxon.VernacularName))
            .GroupBy(
                taxon => taxon.VernacularName!,
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.First(),
                StringComparer.OrdinalIgnoreCase);

        subtaxaByParentId = taxonList
            .Where(taxon => !string.IsNullOrWhiteSpace(taxon.ParentTaxonId))
            .GroupBy(
                taxon => taxon.ParentTaxonId!,
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.ToList(),
                StringComparer.OrdinalIgnoreCase);
    }

    public Taxon? GetById(string taxonId)
    {
        if (string.IsNullOrWhiteSpace(taxonId))
        {
            return null;
        }

        return taxaById.GetValueOrDefault(taxonId);
    }

    public Taxon? GetByVernacularName(string vernacularName)
    {
        if (string.IsNullOrWhiteSpace(vernacularName))
        {
            return null;
        }

        return taxaByVernacularName.GetValueOrDefault(vernacularName);
    }

    public Taxon? GetSupertaxon(string taxonId)
    {
        var taxon = GetById(taxonId);

        if (taxon is null || string.IsNullOrWhiteSpace(taxon.ParentTaxonId))
        {
            return null;
        }

        return GetById(taxon.ParentTaxonId);
    }

    public IReadOnlyCollection<Taxon> GetSubtaxa(string taxonId)
    {
        if (string.IsNullOrWhiteSpace(taxonId))
        {
            return Array.Empty<Taxon>();
        }

        return subtaxaByParentId.TryGetValue(taxonId, out var children)
            ? children
            : Array.Empty<Taxon>();
    }
}