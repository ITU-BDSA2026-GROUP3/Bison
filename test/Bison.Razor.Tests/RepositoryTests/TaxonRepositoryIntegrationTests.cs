using Bison.Razor.domain_model;
using Bison.Razor.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Bison.Razor.Tests;

[Collection("Sequential Tests")]
public class TaxonRepositoryIntegrationTests : IDisposable
{
    IAuthorRepository _authorRepository;
    
    public TaxonRepositoryIntegrationTests()
    {
        DbContextOptionsBuilder<BisonContext> options = new DbContextOptionsBuilder<BisonContext>();
        options.UseSqlite("../../Data/test.db");
        BisonContext bc = new  BisonContext(options.Options);

        bc.Database.EnsureCreated();

        _authorRepository = new AuthorRepository(bc);
    }

    [Fact]
    public void CreateSingleTaxon()
    {
        
    }

    [Fact]
    public void CreateMultipleTaxons()
    {
        
    }

    [Fact]
    public void GetTaxonByValidId()
    {
        
    }

    [Fact]
    public void GetTaxonByInvalidId()
    {
        
    }

    [Fact]
    public void GetAllTaxonsFromDatabase()
    {
        
    }

    [Fact]
    public void GetAllTaxonsFromEmptyDatabase()
    {
        
    }

    public void Dispose()
    {
        
    }
}