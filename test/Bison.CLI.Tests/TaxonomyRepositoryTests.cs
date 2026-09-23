using Bison.Taxonomy;

namespace Bison.CLI.Tests;

[Collection("Sequential Tests")]
public class TaxonomyRepositoryTests
{
    private readonly ITaxonomyRepository repository;

    public TaxonomyRepositoryTests()
    {
        Taxon order = new()
        {
            TaxonId = "order-1",
            ScientificName = "Pelecaniformes",
            TaxonRank = "order"
        };

        Taxon genus = new()
        {
            TaxonId = "genus-1",
            ParentTaxonId = "order-1",
            ScientificName = "Ardea",
            TaxonRank = "genus"
        };

        Taxon species = new()
        {
            TaxonId = "species-1",
            ParentTaxonId = "genus-1",
            ScientificName = "Ardea cinerea",
            VernacularName = "Fiskehejre",
            TaxonRank = "species"
        };

        repository = new TaxonomyRepository(
            new[] { order, genus, species });
    }

    [Fact]
    public void GetById_ReturnsMatchingTaxon()
    {
        Taxon? result = repository.GetById("species-1");

        Assert.NotNull(result);
        Assert.Equal("Ardea cinerea", result.ScientificName);
    }

    [Fact]
    public void GetByVernacularName_ReturnsMatchingTaxon()
    {
        Taxon? result =
            repository.GetByVernacularName("Fiskehejre");

        Assert.NotNull(result);
        Assert.Equal("Ardea cinerea", result.ScientificName);
    }

    [Fact]
    public void GetSupertaxon_ReturnsDirectParent()
    {
        Taxon? result = repository.GetSupertaxon("species-1");

        Assert.NotNull(result);
        Assert.Equal("Ardea", result.ScientificName);
    }

    [Fact]
    public void GetSubtaxa_ReturnsDirectChildren()
    {
        IReadOnlyCollection<Taxon> result =
            repository.GetSubtaxa("genus-1");

        Taxon child = Assert.Single(result);
        Assert.Equal("Ardea cinerea", child.ScientificName);
    }

    [Fact]
    public void UnknownTaxonId_ReturnsNull()
    {
        Taxon? result = repository.GetById("unknown");

        Assert.Null(result);
    }

    [Fact]
    public void TaxonWithoutSubtaxa_ReturnsEmptyCollection()
    {
        IReadOnlyCollection<Taxon> result =
            repository.GetSubtaxa("species-1");

        Assert.Empty(result);
    }
}