using System.Globalization;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;

namespace Bison.Taxonomy;

public static class TaxonomyCsvLoader
{
    public static IReadOnlyCollection<Taxon> Load()
    {
        Assembly assembly = typeof(TaxonomyCsvLoader).Assembly;

        string resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name =>
                name.EndsWith("joined.csv", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException(
                "The embedded taxonomy resource 'joined.csv' could not be found.");

        using Stream stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"The embedded resource '{resourceName}' could not be opened.");

        using StreamReader reader = new(stream);

        CsvConfiguration configuration = new(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            TrimOptions = TrimOptions.Trim
        };

        using CsvReader csv = new(reader, configuration);

        csv.Context.RegisterClassMap<TaxonCsvMap>();

        return csv.GetRecords<Taxon>().ToList();
    }

    private sealed class TaxonCsvMap : ClassMap<Taxon>
    {
        public TaxonCsvMap()
        {
            Map(taxon => taxon.TaxonId)
                .Name("dwc:taxonID");

            Map(taxon => taxon.ParentTaxonId)
                .Name("dwc:parentNameUsageID")
                .Optional();

            Map(taxon => taxon.TaxonRank)
                .Name("dwc:taxonRank");

            Map(taxon => taxon.ScientificName)
                .Name("dwc:scientificName");

            Map(taxon => taxon.VernacularName)
                .Name("dwc:vernacularName")
                .Optional();
        }
    }
}