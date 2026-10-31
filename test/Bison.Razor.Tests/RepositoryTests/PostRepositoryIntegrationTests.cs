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
    IPostRepository _postRepository;
    
    public PostRepositoryIntegrationTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<BisonContext>()
            .UseSqlite(_connection)
            .Options;

        BisonContext bc = new BisonContext(_contextOptions);

        bc.Database.EnsureCreated();

        _postRepository = new PostRepository(bc);
    }

    [Fact]
    public void CreateSingleObservation()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void CreateMultipleObservations()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void GetObservationByValidId()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void GetObservationByInvalidId()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void GetAllObservationsFromDatabase()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void GetAllObservationsFromEmptyDatabase()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void CreateSingleComment()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void CreateMultipleComments()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void GetSingleCommentFromObservation()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void GetMultipleCommentsFromObservation()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void GetCommentsFromNonExistingObservation()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void CreateSingleCProposal()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void CreateMultipleProposals()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void GetSingleProposalFromObservation()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void GetMultipleProposalsFromObservation()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void GetProposalsFromNonExistingObservation()
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