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

        var comment1 = new Comment
        {
            Text = "Nice observation!",
            TimeStamp = DateTime.Now,
            Author = author2,
            Observation = observation1
        };

        var comment2 = new Comment
        {
            Text = "Where exactly did you see it?",
            TimeStamp = DateTime.Now,
            Author = author1,
            Observation = observation2
        };

        context.Comments.AddRange(
            comment1,
            comment2);

        context.SaveChanges();

        var proposal1 = new Proposal
        {
            Text = "I think this might actually be a Bison",
            TimeStamp = DateTime.Now,
            Author = author2,
            Observation = observation1,
            Taxon = taxon1
        };

        var proposal2 = new Proposal
        {
            Text = "This observation could be a Heron",
            TimeStamp = DateTime.Now,
            Author = author1,
            Observation = observation2,
            Taxon = taxon2
        };

        context.Proposals.AddRange(
            proposal1,
            proposal2);

        context.SaveChanges();
    }
}