using System.Data.Common;
using Bison.Razor.domain_model;
using Bison.Razor.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Bison.Razor.Tests;

[Collection("Sequential Tests")]
public class PostRepositoryIntegrationTests : IDisposable
{
    DbConnection _connection;
    DbContextOptions<BisonContext> _contextOptions;
    BisonContext bc;
    IPostRepository _postRepository;
    
    public PostRepositoryIntegrationTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<BisonContext>()
            .UseSqlite(_connection)
            .Options;

        bc = new BisonContext(_contextOptions);

        bc.Database.EnsureCreated();

        _postRepository = new PostRepository(bc);
    }

    [Fact]
    public async Task CreateSingleObservation()
    {
        //Arrange
        Author author = new Author{UserName = "Emil"};
        Taxon taxon = new Taxon{Name = "Taxon"};
        Observation obs = new Observation{Author = author, Taxon = taxon, Text = "I observe", TimeStamp = DateTime.Now};

        //Act
        await _postRepository.CreateObservations([obs]);
        var records = await bc.Observations.ToListAsync();

        //Assert
        Assert.NotEmpty(records);
        Assert.Single(records);

        CompareObservations(obs, records[0]);
    }

    [Fact]
    public async Task CreateMultipleObservations()
    {
        //Arrange
        Author author = new Author{UserName = "Emil"};
        Taxon taxon = new Taxon{Name = "Taxon"};
        Observation obs1 = new Observation{Author = author, Taxon = taxon, Text = "I observe", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author, Taxon = taxon, Text = "I observed", TimeStamp = DateTime.Now};

        //Act
        await _postRepository.CreateObservations([obs1, obs2]);
        var records = await bc.Observations.ToListAsync();

        //Assert
        Assert.NotEmpty(records);
        Assert.Equal(2, records.Count);

        var record1 = Assert.Single(records.Where(o => o.Id == obs1.Id));
        var record2 = Assert.Single(records.Where(o => o.Id == obs2.Id));

        CompareObservations(obs1, record1);
        CompareObservations(obs2, record2);
    }

    [Fact]
    public async Task GetObservationByValidId()
    {
        //Arrange
        Author author = new Author{UserName = "Emil"};
        Taxon taxon = new Taxon{Name = "Taxon"};
        Observation obs1 = new Observation{Author = author, Taxon = taxon, Text = "I observe", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author, Taxon = taxon, Text = "I observed", TimeStamp = DateTime.Now};
        await _postRepository.CreateObservations([obs1, obs2]);

        //Act
        var record = await _postRepository.ReadSingleObservation(obs1.Id);

        //Assert
        Assert.NotNull(record);
        
        CompareObservations(obs1, record);
    }

    [Fact]
    public async Task GetObservationByInvalidId()
    {
        //Arrange
        Author author = new Author{UserName = "Emil"};
        Taxon taxon = new Taxon{Name = "Taxon"};
        Observation obs = new Observation{Author = author, Taxon = taxon, Text = "I observe", TimeStamp = DateTime.Now};
        await _postRepository.CreateObservations([obs]);

        //Act
        var record = await _postRepository.ReadSingleObservation(-51515);

        //Assert
        Assert.Null(record);
    }

    [Fact]
    public async Task GetAllObservationsFromDatabase()
    {
        //Arrange
        Author author = new Author{UserName = "Emil"};
        Taxon taxon = new Taxon{Name = "Taxon"};
        Observation obs1 = new Observation{Author = author, Taxon = taxon, Text = "I observe", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author, Taxon = taxon, Text = "I observed", TimeStamp = DateTime.Now};
        await _postRepository.CreateObservations([obs1, obs2]);

        //Act
        var records = await _postRepository.ReadAllObservations();

        //Assert
        Assert.NotEmpty(records);
        Assert.Equal(2, records.Count);

        var record1 = Assert.Single(records.Where(o => o.Id == obs1.Id));
        var record2 = Assert.Single(records.Where(o => o.Id == obs2.Id));

        CompareObservations(obs1, record1);
        CompareObservations(obs2, record2);
    }

    [Fact]
    public async Task GetAllObservationsFromEmptyDatabase()
    {
        //Arrange

        //Act
        var records = await _postRepository.ReadAllObservations();

        //Assert
        Assert.Empty(records);
    }

    [Fact]
    public async Task CreateSingleComment()
    {
        //Arrange
        Author author = new Author{UserName = "Emil"};
        Taxon taxon = new Taxon{Name = "Taxon"};
        Observation obs = new Observation{Author = author, Taxon = taxon, Text = "I observe", TimeStamp = DateTime.Now};
        Comment com = new Comment{Author = author, Observation = obs, Text = "I cant believe you just did that"};

        //Act
        await _postRepository.CreateComments([com]);
        var records = await bc.Comments.ToListAsync();

        //Assert
        Assert.NotEmpty(records);
        Assert.Single(records);

        CompareComments(com, records[0]);
    }

    [Fact]
    public async Task CreateMultipleComments()
    {
        //Arrange
        Author author = new Author{UserName = "Emil"};
        Taxon taxon = new Taxon{Name = "Taxon"};
        Observation obs = new Observation{Author = author, Taxon = taxon, Text = "I observe", TimeStamp = DateTime.Now};
        Comment com1 = new Comment{Author = author, Observation = obs, Text = "I cant believe you just did that"};
        Comment com2 = new Comment{Author = author, Observation = obs, Text = "He didn't just do what i think he did did he?"};

        //Act
        await _postRepository.CreateComments([com1, com2]);
        var records = await bc.Comments.ToListAsync();

        //Assert
        Assert.NotEmpty(records);
        Assert.Equal(2, records.Count);

        var record1 = Assert.Single(records.Where(o => o.Id == com1.Id));
        var record2 = Assert.Single(records.Where(o => o.Id == com2.Id));

        CompareComments(com1, record1);
        CompareComments(com2, record2);
    }

    [Fact]
    public async Task GetSingleCommentFromObservation()
    {
        //Arrange
        Author author = new Author{UserName = "Emil"};
        Taxon taxon = new Taxon{Name = "Taxon"};
        Observation obs1 = new Observation{Author = author, Taxon = taxon, Text = "I observe", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author, Taxon = taxon, Text = "I observe but again", TimeStamp = DateTime.Now};
        Comment com = new Comment{Author = author, Observation = obs1, Text = "I cant believe you just did that"};

        await _postRepository.CreateComments([com]);

        //Act
        var records = await _postRepository.ReadComments(obs1.Id);

        //Assert
        Assert.NotEmpty(records);
        Assert.Single(records);

        CompareComments(com, records[0]);
    }

    [Fact]
    public async Task GetMultipleCommentsFromObservation()
    {
        //Arrange
        Author author = new Author{UserName = "Emil"};
        Taxon taxon = new Taxon{Name = "Taxon"};
        Observation obs1 = new Observation{Author = author, Taxon = taxon, Text = "I observe", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author, Taxon = taxon, Text = "I observe but again", TimeStamp = DateTime.Now};
        Comment com1 = new Comment{Author = author, Observation = obs1, Text = "I cant believe you just did that"};
        Comment com2 = new Comment{Author = author, Observation = obs2, Text = "Someone has to stop him"};
        Comment com3 = new Comment{Author = author, Observation = obs1, Text = "his power is nearly incomprehensable"};

        await _postRepository.CreateComments([com1, com2, com3]);

        //Act
        var records = await _postRepository.ReadComments(obs1.Id);

        //Assert
        Assert.NotEmpty(records);
        Assert.Equal(2, records.Count);

        var record1 = Assert.Single(records.Where(o => o.Id == com1.Id));
        var record2 = Assert.Single(records.Where(o => o.Id == com3.Id));

        CompareComments(com1, record1);
        CompareComments(com3, record2);
    }

    [Fact]
    public async Task GetCommentsFromNonExistingObservation()
    {
        //Arrange
        Author author = new Author{UserName = "Emil"};
        Taxon taxon = new Taxon{Name = "Taxon"};
        Observation obs1 = new Observation{Author = author, Taxon = taxon, Text = "I observe", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author, Taxon = taxon, Text = "I observe but again", TimeStamp = DateTime.Now};
        Comment com1 = new Comment{Author = author, Observation = obs1, Text = "I cant believe you just did that"};
        Comment com2 = new Comment{Author = author, Observation = obs2, Text = "Someone has to stop him"};
        Comment com3 = new Comment{Author = author, Observation = obs1, Text = "his power is nearly incomprehensable"};

        await _postRepository.CreateComments([com1, com2, com3]);

        //Act
        var records = await _postRepository.ReadComments(-123454321);

        //Assert
        Assert.Empty(records);
    }

    [Fact]
    public async Task CreateSingleProposal()
    {
        //Arrange
        Author author = new Author{UserName = "Emil"};
        Taxon taxon = new Taxon{Name = "Taxon"};
        Observation obs = new Observation{Author = author, Taxon = taxon, Text = "I observe", TimeStamp = DateTime.Now};
        Proposal prop = new Proposal{Author = author, Observation = obs, Text = "Yes you did, and it was an animal", Taxon = taxon};

        //Act
        await _postRepository.CreateProposals([prop]);
        var records = await bc.Proposals.ToListAsync();

        //Assert
        Assert.NotEmpty(records);
        Assert.Single(records);

        CompareProposals(prop, records[0]);
    }

    [Fact]
    public async Task CreateMultipleProposals()
    {
        //Arrange
        Author author = new Author{UserName = "Emil"};
        Taxon taxon = new Taxon{Name = "Taxon"};
        Observation obs = new Observation{Author = author, Taxon = taxon, Text = "I observe", TimeStamp = DateTime.Now};
        Proposal prop1 = new Proposal{Author = author, Observation = obs, Text = "Yes you did, and it was an animal", Taxon = taxon};
        Proposal prop2 = new Proposal{Author = author, Observation = obs, Text = "I cant believe you just did that", Taxon = taxon};

        //Act
        await _postRepository.CreateProposals([prop1, prop2]);
        var records = await bc.Proposals.ToListAsync();

        //Assert
        Assert.NotEmpty(records);
        Assert.Equal(2, records.Count);

        var record1 = Assert.Single(records.Where(o => o.Id == prop1.Id));
        var record2 = Assert.Single(records.Where(o => o.Id == prop2.Id));

        CompareProposals(prop1, record1);
        CompareProposals(prop2, record2);
    }

    [Fact]
    public async Task GetSingleProposalFromObservation()
    {
        //Arrange
        Author author = new Author{UserName = "Emil"};
        Taxon taxon = new Taxon{Name = "Taxon"};
        Observation obs1 = new Observation{Author = author, Taxon = taxon, Text = "I observe", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author, Taxon = taxon, Text = "I observed", TimeStamp = DateTime.Now};
        Proposal prop = new Proposal{Author = author, Observation = obs1, Text = "Yes you did, and it was an animal", Taxon = taxon};

        await _postRepository.CreateProposals([prop]);

        //Act
        var records = await _postRepository.ReadProposals(obs1.Id);

        //Assert
        Assert.NotEmpty(records);
        Assert.Single(records);

        CompareProposals(prop, records[0]);
    }

    [Fact]
    public async Task GetMultipleProposalsFromObservation()
    {
        //Arrange
        Author author = new Author{UserName = "Emil"};
        Taxon taxon = new Taxon{Name = "Taxon"};
        Observation obs1 = new Observation{Author = author, Taxon = taxon, Text = "I observe", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author, Taxon = taxon, Text = "I observed", TimeStamp = DateTime.Now};
        Proposal prop1 = new Proposal{Author = author, Observation = obs1, Text = "Yes you did, and it was an animal", Taxon = taxon};
        Proposal prop2 = new Proposal{Author = author, Observation = obs1, Text = "I cant believe you just did that", Taxon = taxon};
        Proposal prop3 = new Proposal{Author = author, Observation = obs2, Text = "STOP HIM BEFORE HIS POWER KILLS US", Taxon = taxon};

        await _postRepository.CreateProposals([prop1, prop2, prop3]);

        //Act
        var records = await _postRepository.ReadProposals(obs1.Id);

        //Assert
        Assert.NotEmpty(records);
        Assert.Equal(2, records.Count);

        var record1 = Assert.Single(records.Where(o => o.Id == prop1.Id));
        var record2 = Assert.Single(records.Where(o => o.Id == prop2.Id));

        CompareProposals(prop1, record1);
        CompareProposals(prop2, record2);
    }

    [Fact]
    public async Task GetProposalsFromNonExistingObservation()
    {
        //Arrange
        Author author = new Author{UserName = "Emil"};
        Taxon taxon = new Taxon{Name = "Taxon"};
        Observation obs1 = new Observation{Author = author, Taxon = taxon, Text = "I observe", TimeStamp = DateTime.Now};
        Observation obs2 = new Observation{Author = author, Taxon = taxon, Text = "I observed", TimeStamp = DateTime.Now};
        Proposal prop1 = new Proposal{Author = author, Observation = obs1, Text = "Yes you did, and it was an animal", Taxon = taxon};
        Proposal prop2 = new Proposal{Author = author, Observation = obs1, Text = "I cant believe you just did that", Taxon = taxon};
        Proposal prop3 = new Proposal{Author = author, Observation = obs2, Text = "STOP HIM BEFORE HIS POWER KILLS US", Taxon = taxon};

        await _postRepository.CreateProposals([prop1, prop2, prop3]);

        //Act
        var records = await _postRepository.ReadProposals(-456576447);

        //Assert
        Assert.Empty(records);
    }

    private void CompareObservations(Observation obs1, Observation obs2)
    {
        Assert.NotNull(obs1);
        Assert.NotNull(obs2);

        Assert.Equal(obs1.Id, obs2.Id);
        Assert.Equal(obs1.Author, obs2.Author);
        Assert.Equal(obs1.Text, obs2.Text);
        Assert.Equal(obs1.TimeStamp, obs2.TimeStamp);
        Assert.Equal(obs1.Taxon, obs2.Taxon);
    }

    private void CompareComments(Comment com1, Comment com2)
    {
        Assert.NotNull(com1);
        Assert.NotNull(com2);

        Assert.Equal(com1.Id, com2.Id);
        Assert.Equal(com1.Text, com2.Text);
        Assert.Equal(com1.Observation, com2.Observation);
        Assert.Equal(com1.Author, com2.Author);
        Assert.Equal(com1.TimeStamp, com2.TimeStamp);
    }

    private void CompareProposals(Proposal prop1, Proposal prop2)
    {
        Assert.NotNull(prop1);
        Assert.NotNull(prop2);

        Assert.Equal(prop1.Id, prop2.Id);
        Assert.Equal(prop1.Taxon, prop2.Taxon);
        Assert.Equal(prop1.Observation, prop2.Observation);
        Assert.Equal(prop1.Text, prop2.Text);
        Assert.Equal(prop1.Author, prop2.Author);
        Assert.Equal(prop1.TimeStamp, prop2.TimeStamp);
    }

    public void Dispose()
    {
        bc.Dispose();
        _connection.Dispose();
    }
}