using Microsoft.EntityFrameworkCore;

public class BisonContext : DbContext
{
    public DbSet<Author> Authors
    {
        get; set;
    }
    public DbSet<Comment> Comments
    {
        get; set;
    }
    public DbSet<Observation> Observations
    {
        get; set;
    }
    public DbSet<Proposal> Proposals
    {
        get; set;
    }
    public DbSet<Taxon> Taxons // The scientifically accepted plural form would be "taxa"
    {
        get; set;
    }

    public BisonContext(DbContextOptions<BisonContext> options) : base(options)
    {

    }
}