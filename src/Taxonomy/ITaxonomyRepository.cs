namespace Bison.Taxonomy;

public interface ITaxonomyRepository
{
    Taxon? GetById(string taxonId);

    Taxon? GetByVernacularName(string vernacularName);

    Taxon? GetSupertaxon(string taxonId);

    IReadOnlyCollection<Taxon> GetSubtaxa(string taxonId);
}