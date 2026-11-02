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
    BisonContext bc;
    ITaxonRepository _taxonRepository;
    
    public TaxonRepositoryIntegrationTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<BisonContext>()
            .UseSqlite(_connection)
            .Options;

        bc = new BisonContext(_contextOptions);

        bc.Database.EnsureCreated();

        _taxonRepository = new TaxonRepository(bc);
    }

    [Fact]
    public async Task CreateSingleTaxon()
    {
        //Arrange
        Taxon taxon = new Taxon{Name = "Cool animal", Description = "EPIC ANIMAL"};

        //Act
        await _taxonRepository.CreateTaxons([taxon]);
        var records = await bc.Taxons.ToListAsync();

        //Assert
        Assert.NotEmpty(records);
        Assert.Single(records);

        CompareTaxons(taxon, records[0]);
    }

    [Fact]
    public async Task CreateMultipleTaxons()
    {
        //Arrange
        Taxon taxon1 = new Taxon{Name = "Cool animal", Description = "EPIC ANIMAL"};
        Taxon taxon2 = new Taxon{Name = "Cooler animal", Description = "SUPER EPIC ANIMAL"};

        //Act
        await _taxonRepository.CreateTaxons([taxon1, taxon2]);
        var records = await bc.Taxons.ToListAsync();

        //Assert
        Assert.NotEmpty(records);
        Assert.Equal(2, records.Count);

        var record1 = Assert.Single(records.Where(o => o.Id == taxon1.Id));
        var record2 = Assert.Single(records.Where(o => o.Id == taxon2.Id));

        CompareTaxons(taxon1, record1);
        CompareTaxons(taxon2, record2);
    }

    [Fact]
    public async Task GetTaxonByValidId()
    {
        //Arrange
        Taxon taxon1 = new Taxon{Name = "Cool animal", Description = "EPIC ANIMAL"};
        Taxon taxon2 = new Taxon{Name = "Cooler animal", Description = "SUPER EPIC ANIMAL"};
        await _taxonRepository.CreateTaxons([taxon1, taxon2]);

        //Act
        var record = await _taxonRepository.getTaxon(taxon1.Id);

        //Assert
        Assert.NotNull(record);

        CompareTaxons(taxon1, record);
    }

    [Fact]
    public async Task GetTaxonByInvalidId()
    {
        //Arrange
        Taxon taxon1 = new Taxon{Name = "Cool animal", Description = "EPIC ANIMAL"};
        Taxon taxon2 = new Taxon{Name = "Cooler animal", Description = "SUPER EPIC ANIMAL"};
        await _taxonRepository.CreateTaxons([taxon1, taxon2]);

        //Act
        var record = await _taxonRepository.getTaxon(-12328173);

        //Assert
        Assert.Null(record);
    }

    [Fact]
    public async Task GetAllTaxonsFromDatabase()
    {
        //Arrange
        Taxon taxon1 = new Taxon{Name = "Cool animal", Description = "EPIC ANIMAL"};
        Taxon taxon2 = new Taxon{Name = "Cooler animal", Description = "SUPER EPIC ANIMAL"};
        await _taxonRepository.CreateTaxons([taxon1, taxon2]);

        //Act
        var records = await _taxonRepository.getAllTaxons();

        //Assert
        Assert.NotEmpty(records);

        var record1 = Assert.Single(records.Where(o => o.Id == taxon1.Id));
        var record2 = Assert.Single(records.Where(o => o.Id == taxon2.Id));

        CompareTaxons(taxon1, record1);
        CompareTaxons(taxon2, record2);
    }

    [Fact]
    public async Task GetAllTaxonsFromEmptyDatabase()
    {
        //Arrange


        //Act
        var records = await _taxonRepository.getAllTaxons();

        //Assert
        Assert.Empty(records);
    }

    private void CompareTaxons(Taxon tax1, Taxon tax2)
    {
        Assert.NotNull(tax1);
        Assert.NotNull(tax1);

        Assert.Equal(tax1.Id, tax2.Id);
        Assert.Equal(tax1.Name, tax2.Name);
        Assert.Equal(tax1.ancestor, tax2.ancestor);
    }

    public void Dispose()
    {
        bc.Dispose();
        _connection.Dispose();
    }
}