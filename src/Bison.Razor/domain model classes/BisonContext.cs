using Microsoft.EntityFrameworkCore;

public class BisonContext : DbContext
{
    public DbSet<Message> Messages
    {
        get; set;
    }
    public DbSet<User> Users
    {
        get; set;
    }

    public BisonContext(DbContextOptions<BisonContext> options) : base(options)
    {

    }
}