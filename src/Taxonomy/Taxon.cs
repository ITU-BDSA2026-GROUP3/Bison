namespace Bison.Taxonomy;

public sealed class Taxon
{
    public string TaxonId { get; init; } = string.Empty;

    public string? ParentTaxonId { get; init; }

    public string TaxonRank { get; init; } = string.Empty;

    public string ScientificName { get; init; } = string.Empty;

    public string? VernacularName { get; init; }
}