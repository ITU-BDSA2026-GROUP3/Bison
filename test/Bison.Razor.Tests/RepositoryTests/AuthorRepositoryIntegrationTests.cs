using Bison.Razor.domain_model;
using Bison.Razor.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Bison.Razor.Tests;

[Collection("Sequential Tests")]
public class AuthorRepositoryIntegrationTests : IDisposable
{
    IAuthorRepository _authorRepository;
    
    public AuthorRepositoryIntegrationTests()
    {
        DbContextOptionsBuilder<BisonContext> options = new DbContextOptionsBuilder<BisonContext>();
        options.UseSqlite("../../Data/test.db");
        BisonContext bc = new  BisonContext(options.Options);

        bc.Database.EnsureCreated();

        _authorRepository = new AuthorRepository(bc);
    }

    [Fact]
    public void CreateSingleAuthor()
    {
        
    }

    [Fact]
    public void CreateMultipleAuthors()
    {
        
    }

    [Fact]
    public void GetAuthorByValidId()
    {
        
    }

    [Fact]
    public void GetAuthorByInvalidId()
    {
        
    }

    public void Dispose()
    {
        
    }
}