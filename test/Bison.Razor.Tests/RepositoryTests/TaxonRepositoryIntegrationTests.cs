using System.Data.Common;
using Bison.Razor.domain_model;
using Bison.Razor.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Bison.Razor.Tests;

[Collection("Sequential Tests")]
public class TaxonRepositoryIntegrationTests : IDisposable
{
    DbConnection _connection;
    DbContextOptions<BisonContext> _contextOptions;
    ITaxonRepository _taxonRepository;
    
    public TaxonRepositoryIntegrationTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<BisonContext>()
            .UseSqlite(_connection)
            .Options;

        BisonContext bc = new BisonContext(_contextOptions);

        bc.Database.EnsureCreated();

        _taxonRepository = new TaxonRepository(bc);
    }

    [Fact]
    public void CreateSingleTaxon()
    {
        //Arrange


        //Act


        //Assert

    }

    [Fact]
    public void CreateMultipleTaxons()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void GetTaxonByValidId()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void GetTaxonByInvalidId()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void GetAllTaxonsFromDatabase()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void GetAllTaxonsFromEmptyDatabase()
    {
        //Arrange


        //Act


        //Assert
        
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}