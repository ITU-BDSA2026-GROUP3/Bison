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

        var observation1 = new Observation
        {
            Text = "I saw a heron near the lake",
            TimeStamp = DateTime.Now,
            Author = author1,
            Taxon = taxon2
        };

        var observation2 = new Observation
        {
            Text = "There is a bison in the field",
            TimeStamp = DateTime.Now,
            Author = author2,
            Taxon = taxon1
        };

        context.Observations.AddRange(
            observation1,
            observation2);

        context.SaveChanges();
    }
}