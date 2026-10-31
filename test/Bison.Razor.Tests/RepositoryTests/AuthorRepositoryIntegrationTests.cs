using System.Data.Common;
using Bison.Razor.domain_model;
using Bison.Razor.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Bison.Razor.Tests;

[Collection("Sequential Tests")]
public class AuthorRepositoryIntegrationTests : IDisposable
{
    DbConnection _connection;
    DbContextOptions<BisonContext> _contextOptions;
    BisonContext bc;
    IAuthorRepository _authorRepository;
    
    public AuthorRepositoryIntegrationTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<BisonContext>()
            .UseSqlite(_connection)
            .Options;

        bc = new BisonContext(_contextOptions);

        bc.Database.EnsureCreated();

        _authorRepository = new AuthorRepository(bc);
    }

    [Fact]
    public async Task CreateSingleAuthor()
    {
        //Arrange
        Author author = new Author{Name = "Emil", Email = "Email@email.email", Posts = new List<Post>()};

        //Act
        await _authorRepository.CreateAuthors([author]);
        var records = await bc.Authors.ToListAsync();

        //Assert
        Assert.NotEmpty(records);
        Assert.Single(records);

        CompareAuthors(author, records[0]);
    }

    [Fact]
    public async Task CreateMultipleAuthors()
    {
        //Arrange
        Author author1 = new Author{Name = "Emil", Email = "Email@email.email", Posts = new List<Post>()};
        Author author2 = new Author{Name = "Villads", Email = "Villads@villads.villads", Posts = new List<Post>()};

        //Act
        await _authorRepository.CreateAuthors([author1, author2]);
        var records = await bc.Authors.ToListAsync();

        //Assert
        Assert.NotEmpty(records);
        Assert.Equal(2, records.Count);

        var record1 = Assert.Single(records.Where(o => o.Id == author1.Id));
        var record2 = Assert.Single(records.Where(o => o.Id == author2.Id));

        CompareAuthors(author1, record1);
        CompareAuthors(author2, record2);
    }

    [Fact]
    public async Task PostsSavedWhenCreatingAuthor()
    {
        //Arrange
        Taxon taxon = new Taxon{Name = "Taxon"};
        Observation obs1 = new Observation{Text = "Hej", Taxon = taxon};
        Observation obs2 = new Observation{Text = "WOW", Taxon = taxon};
        Author author = new Author{Name = "Emil", Email = "Email@email.email", Posts = new List<Post>(){obs1, obs2}};
        obs1.Author = author;
        obs2.Author = author;

        //Act
        await _authorRepository.CreateAuthors([author]);
        var records = await bc.Authors.ToListAsync();

        //Assert
        Assert.NotEmpty(records);
        Assert.Single(records);

        CompareAuthors(author, records[0]);

        var posts = records[0].Posts;

        var post1 = Assert.Single(posts.Where(o => o.Id == obs1.Id));
        var post2 = Assert.Single(posts.Where(o => o.Id == obs2.Id));

        Assert.Equal(obs1.Id, post1.Id);
        Assert.Equal(obs1.Text, post1.Text);
        Assert.Equal(obs2.Id, post2.Id);
        Assert.Equal(obs2.Text, post2.Text);
    }

    [Fact]
    public async Task GetAuthorByValidId()
    {
        //Arrange
        Author author1 = new Author{Name = "Waygal", Email = "Waygal@waygal.waygal", Posts = new List<Post>()};
        Author author2 = new Author{Name = "Rune", Email = "Rune@rune.rune", Posts = new List<Post>()};
        await _authorRepository.CreateAuthors([author1, author2]);

        //Act
        var record = await _authorRepository.getAuthor(author1.Id);

        //Assert
        Assert.NotNull(record);

        CompareAuthors(author1, record);
    }

    [Fact]
    public async Task GetAuthorByInvalidId()
    {
        //Arrange
        Author author1 = new Author{Name = "Emil", Email = "Jonas@villads.waygal", Posts = new List<Post>()};
        Author author2 = new Author{Name = "Rune", Email = "Emil@jonas.villads", Posts = new List<Post>()};
        await _authorRepository.CreateAuthors([author1, author2]);

        //Act
        var record = await _authorRepository.getAuthor(-67);

        //Assert
        Assert.Null(record);
    }

    private void CompareAuthors(Author author1, Author author2)
    {
        Assert.NotNull(author1);
        Assert.NotNull(author1);

        Assert.Equal(author1.Id, author2.Id);
        Assert.Equal(author1.Name, author2.Name);
        Assert.Equal(author1.Email, author2.Email);
    }

    public void Dispose()
    {
        bc.Dispose();
        _connection.Dispose();
    }
}