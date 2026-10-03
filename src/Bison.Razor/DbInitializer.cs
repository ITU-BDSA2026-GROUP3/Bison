public static class DbInitializer
{
    public static void Initialize(BisonContext context)
    {
        if (context.Authors.Any())
        {
            return;
        }

        var author1 = new Author
        {
            Name = "Peter",
            Email = "peter@example.com"
        };

        var author2 = new Author
        {
            Name = "Paul",
            Email = "paul@example.com"
        };

        var taxon1 = new Taxon
        {
            Name = "Bison",
            Description = "European bison"
        };

        var taxon2 = new Taxon
        {
            Name = "Heron",
            Description = "Large water bird"
        };

        context.Authors.AddRange(author1, author2);
        context.Taxons.AddRange(taxon1, taxon2);

        context.SaveChanges();
    }
}