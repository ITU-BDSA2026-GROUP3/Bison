using System.Data.Common;
using Bison.Razor.domain_model;
using Bison.Razor.DTOs;
using Bison.Razor.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Bison.Razor.Tests;

[Collection("Sequential Tests")]
public class ObservationServiceIntegrationTests : IDisposable
{
    DbConnection _connection;
    DbContextOptions<BisonContext> _contextOptions;
    BisonContext bc;
    IObservationService service;
    
    public ObservationServiceIntegrationTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<BisonContext>()
            .UseSqlite(_connection)
            .Options;

        bc = new BisonContext(_contextOptions);

        bc.Database.EnsureDeleted();
        bc.Database.EnsureCreated();

        IPostRepository _postRepository = new PostRepository(bc);
        IAuthorRepository _authorRepository = new AuthorRepository(bc);
        ITaxonRepository _taxonRepository = new TaxonRepository(bc);

        service = new ObservationService(_postRepository, _authorRepository, _taxonRepository);
    }

    [Fact]
    public async Task GetObservationsFromDatabase()
    {
        //Arrange
        Author author = new Author{UserName = "Emil"};
        Taxon taxon = new Taxon{Name = "Cool taxon"};
        Observation obs1 = new Observation{Author = author, Taxon = taxon, Text = "Wow", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author, Taxon = taxon, Text = "coolmand", TimeStamp = DateTime.Now};
        bc.AddRange(author, taxon, obs1, obs2);
        bc.SaveChanges();

        //Act
        var records = await service.GetObservations();

        //Assert
        Assert.NotEmpty(records);
        Assert.Equal(2, records.Count);

        var record1 = Assert.Single(records.Where(o => o.Id == obs1.Id));
        var record2 = Assert.Single(records.Where(o => o.Id == obs2.Id));

        CompareObservations(obs1, record1);
        CompareObservations(obs2, record2);
    }

    [Fact]
    public async Task GetObservationsFromEmptyDatabase()
    {
        //Arrange


        //Act
        var records = await service.GetObservations();

        //Assert
        Assert.Empty(records);
    }

    [Fact]
    public async Task GetObservationByValidId()
    {
        //Arrange
        Author author = new Author{UserName = "Emil"};
        Taxon taxon = new Taxon{Name = "Cool taxon"};
        Observation obs1 = new Observation{Author = author, Taxon = taxon, Text = "Wow", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author, Taxon = taxon, Text = "coolmand", TimeStamp = DateTime.Now};
        bc.AddRange(author, taxon, obs1, obs2);
        bc.SaveChanges();

        //Act
        var record = await service.GetObservationFromId(obs1.Id);

        //Assert
        Assert.NotNull(record);

        CompareObservations(obs1, record);
    }

    [Fact]
    public async Task GetObservationByInvalidId()
    {
        //Arrange
        Author author = new Author{UserName = "Emil"};
        Taxon taxon = new Taxon{Name = "Cool taxon"};
        Observation obs1 = new Observation{Author = author, Taxon = taxon, Text = "Wow", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author, Taxon = taxon, Text = "coolmand", TimeStamp = DateTime.Now};
        bc.AddRange(author, taxon, obs1, obs2);
        bc.SaveChanges();

        //Act
        var record = await service.GetObservationFromId(-12315432);

        //Assert
        Assert.Null(record);
    }

    [Fact]
    public async Task GetObservationsByAuthor()
    {
        //Arrange
        Author author1 = new Author{UserName = "Emil"};
        Author author2 = new Author{UserName = "Stabil"};
        Taxon taxon = new Taxon{Name = "Cool taxon"};
        Observation obs1 = new Observation{Author = author1, Taxon = taxon, Text = "Wow", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author1, Taxon = taxon, Text = "coolmand", TimeStamp = DateTime.Now};
        Observation obs3 = new Observation{Author = author2, Taxon = taxon, Text = "ild og is", TimeStamp = DateTime.Now};
        bc.AddRange(author1, author2, taxon, obs1, obs2, obs3);
        bc.SaveChanges();

        //Act
        var records = await service.GetObservationsFromAuthor(author1.Id);

        //Assert
        Assert.NotEmpty(records);
        Assert.Equal(2, records.Count);

        var record1 = Assert.Single(records.Where(o => o.Id == obs1.Id));
        var record2 = Assert.Single(records.Where(o => o.Id == obs2.Id));

        CompareObservations(obs1, record1);
        CompareObservations(obs2, record2);
    }

    [Fact]
    public async Task GetObservationsByNonExistingAuthor()
    {
        //Arrange
        Author author1 = new Author{UserName = "Emil"};
        Author author2 = new Author{UserName = "Stabil"};
        Taxon taxon = new Taxon{Name = "Cool taxon"};
        Observation obs1 = new Observation{Author = author1, Taxon = taxon, Text = "Wow", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author1, Taxon = taxon, Text = "coolmand", TimeStamp = DateTime.Now};
        Observation obs3 = new Observation{Author = author2, Taxon = taxon, Text = "ild og is", TimeStamp = DateTime.Now};
        bc.AddRange(author1, author2, taxon, obs1, obs2, obs3);
        bc.SaveChanges();

        //Act
        var records = await service.GetObservationsFromAuthor("-23");

        //Assert
        Assert.Empty(records);
    }

    [Fact]
    public async Task GetObservationsByTaxon()
    {
        //Arrange
        Author author1 = new Author{UserName = "Emil"};
        Author author2 = new Author{UserName = "Stabil"};
        Taxon taxon1 = new Taxon{Name = "top dawg"};
        Taxon taxon2 = new Taxon{Name = "middle dog", ancestor = taxon1};
        Taxon taxon3 = new Taxon{Name = "lonely dog"};
        Taxon taxon4 = new Taxon{Name = "small dog", ancestor = taxon2};
        Taxon taxon5 = new Taxon{Name = "smallest dog the size of an atom", ancestor = taxon4};
        Observation obs1 = new Observation{Author = author1, Taxon = taxon1, Text = "Wow", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author2, Taxon = taxon2, Text = "coolmand", TimeStamp = DateTime.Now};
        Observation obs3 = new Observation{Author = author2, Taxon = taxon3, Text = "ild og is", TimeStamp = DateTime.Now};
        Observation obs4 = new Observation{Author = author1, Taxon = taxon4, Text = "smack", TimeStamp = DateTime.Now};
        Observation obs5 = new Observation{Author = author1, Taxon = taxon5, Text = "smoll", TimeStamp = DateTime.Now};

        bc.AddRange(author1, author2, taxon1, taxon2, taxon3, taxon4, taxon5, obs1, obs2, obs3, obs4, obs5);
        bc.SaveChanges();

        //Act
        var records = await service.GetObservationsFromTaxon(taxon2.Id);

        //Assert
        Assert.NotEmpty(records);
        Assert.Equal(3, records.Count);

        var record1 = Assert.Single(records.Where(o => o.Id == obs2.Id));
        var record2 = Assert.Single(records.Where(o => o.Id == obs4.Id));
        var record3 = Assert.Single(records.Where(o => o.Id == obs5.Id));

        CompareObservations(obs2, record1);
        CompareObservations(obs4, record2);
        CompareObservations(obs5, record3);
    }

    [Fact]
    public async Task GetObservationsByTaxonNeverUsed()
    {
        //Arrange
        Author author1 = new Author{UserName = "Emil"};
        Author author2 = new Author{UserName = "Stabil"};
        Taxon taxon1 = new Taxon{Name = "top dawg"};
        Taxon taxon2 = new Taxon{Name = "lonely dog"};
        Observation obs1 = new Observation{Author = author1, Taxon = taxon1, Text = "Wow", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author2, Taxon = taxon1, Text = "ild og is", TimeStamp = DateTime.Now};

        bc.AddRange(author1, author2, taxon1, taxon2, obs1, obs2);
        bc.SaveChanges();

        //Act
        var records = await service.GetObservationsFromTaxon(taxon2.Id);

        //Assert
        Assert.Empty(records);
    }

    [Fact]
    public async Task GetObservationsByNonExistingTaxon()
    {
        //Arrange
        Author author1 = new Author{UserName = "Emil"};
        Author author2 = new Author{UserName = "Stabil"};
        Taxon taxon1 = new Taxon{Name = "top dawg"};
        Taxon taxon2 = new Taxon{Name = "lonely dog"};
        Observation obs1 = new Observation{Author = author1, Taxon = taxon1, Text = "Wow", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author2, Taxon = taxon2, Text = "ild og is", TimeStamp = DateTime.Now};

        bc.AddRange(author1, author2, taxon1, taxon2, obs1, obs2);
        bc.SaveChanges();

        //Act
        var records = await service.GetObservationsFromTaxon(-324);

        //Assert
        Assert.Null(records);
    }

    [Fact]
    public async Task GetComments()
    {
        //Arrange
        Author author = new Author{UserName = "Emil"};
        Taxon taxon = new Taxon{Name = "Cool taxon"};
        Observation obs1 = new Observation{Author = author, Taxon = taxon, Text = "Wow", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author, Taxon = taxon, Text = "coolmand", TimeStamp = DateTime.Now};
        Comment com1 = new Comment{Author = author, Observation = obs1, Text = "sejt bro", TimeStamp = DateTime.Now};
        Comment com2 = new Comment{Author = author, Observation = obs1, Text = "sejt bro 2", TimeStamp = DateTime.Now};
        Comment com3 = new Comment{Author = author, Observation = obs2, Text = "sejt bro 3", TimeStamp = DateTime.Now};

        bc.AddRange(author, taxon, obs1, obs2, com1, com2, com3);
        bc.SaveChanges();

        //Act
        var records = await service.GetComments(obs1.Id);

        //Assert
        Assert.NotEmpty(records);
        Assert.Equal(2, records.Count);

        var record1 = Assert.Single(records.Where(o => o.Text == com1.Text));
        var record2 = Assert.Single(records.Where(o => o.Text == com2.Text));

        CompareComments(com1, record1);
        CompareComments(com2, record2);
    }

    [Fact]
    public async Task GetProposals()
    {
        //Arrange
        Author author = new Author{UserName = "Emil"};
        Taxon taxon = new Taxon{Name = "Cool taxon"};
        Observation obs1 = new Observation{Author = author, Taxon = taxon, Text = "Wow", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author, Taxon = taxon, Text = "coolmand", TimeStamp = DateTime.Now};
        Proposal prop1 = new Proposal{Author = author, Observation = obs1, Text = "sejt bro", Taxon = taxon, TimeStamp = DateTime.Now};
        Proposal prop2 = new Proposal{Author = author, Observation = obs1, Text = "sejt bro 2", Taxon = taxon, TimeStamp = DateTime.Now};
        Proposal prop3 = new Proposal{Author = author, Observation = obs2, Text = "sejt bro 3", Taxon = taxon, TimeStamp = DateTime.Now};

        bc.AddRange(author, taxon, obs1, obs2, prop1, prop2, prop3);
        bc.SaveChanges();

        //Act
        var records = await service.GetProposals(obs1.Id);

        //Assert
        Assert.NotEmpty(records);
        Assert.Equal(2, records.Count);

        var record1 = Assert.Single(records.Where(o => o.Text == prop1.Text));
        var record2 = Assert.Single(records.Where(o => o.Text == prop2.Text));

        CompareProposals(prop1, record1);
        CompareProposals(prop2, record2);       
    }

    private void CompareObservations(Observation obs1, ObservationDto obs2)
    {
        Assert.NotNull(obs1);
        Assert.NotNull(obs2);

        Assert.Equal(obs1.Id, obs2.Id);
        Assert.Equal(obs1.Author.UserName, obs2.AuthorName);
        Assert.Equal(obs1.Text, obs2.Text);
        Assert.Equal(obs1.TimeStamp.ToString("MM/dd/yy H:mm:ss"), obs2.Timestamp);
        Assert.Equal(obs1.Taxon.Name, obs2.TaxonName);
    }

    private void CompareComments(Comment com1, CommentDto com2)
    {
        Assert.NotNull(com1);
        Assert.NotNull(com2);

        Assert.Equal(com1.Text, com2.Text);
        Assert.Equal(com1.Author.UserName, com2.AuthorName);
        Assert.Equal(com1.TimeStamp.ToString("MM/dd/yy H:mm:ss"), com2.Timestamp);
    }

    private void CompareProposals(Proposal prop1, ProposalDto prop2)
    {
        Assert.NotNull(prop1);
        Assert.NotNull(prop2);

        Assert.Equal(prop1.Observation.Id, prop2.Id);
        Assert.Equal(prop1.Taxon.Name, prop2.TaxonName);
        Assert.Equal(prop1.Text, prop2.Text);
        Assert.Equal(prop1.Author.UserName, prop2.AuthorName);
        Assert.Equal(prop1.TimeStamp.ToString("MM/dd/yy H:mm:ss"), prop2.Timestamp);
    }

    public void Dispose()
    {
        bc.Dispose();
        _connection.Dispose();
    }
}