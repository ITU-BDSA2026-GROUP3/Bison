using Bison.Razor.domain_model;
using Bison.Razor.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Bison.Razor.Tests;

[Collection("Sequential Tests")]
public class PostRepositoryIntegrationTests : IDisposable
{
    IAuthorRepository _authorRepository;
    
    public PostRepositoryIntegrationTests()
    {
        DbContextOptionsBuilder<BisonContext> options = new DbContextOptionsBuilder<BisonContext>();
        options.UseSqlite("../../Data/test.db");
        BisonContext bc = new  BisonContext(options.Options);

        bc.Database.EnsureCreated();

        _authorRepository = new AuthorRepository(bc);
    }

        /*public Task<List<int>> CreateObservations(List<Observation> observations);

        public Task<Observation> ReadSingleObservation(int observationID);
        public Task<List<Observation>> ReadObservations(int authorID);
        public Task<List<Observation>> ReadAllObservations();

        public Task<List<int>> CreateComments(List<Comment> comments);
        public Task<List<Comment>> ReadComments(int obsID);
        public Task<List<int>> CreateProposals(List<Proposal> proposals);
        public Task<List<Proposal>> ReadProposals(int obsID);
        */

    [Fact]
    public void CreateSingleObservation()
    {
        
    }

    [Fact]
    public void CreateMultipleObservations()
    {
        
    }

    [Fact]
    public void GetObservationByValidId()
    {
        
    }

    [Fact]
    public void GetObservationByInvalidId()
    {
        
    }

    [Fact]
    public void GetAllObservationsFromDatabase()
    {
        
    }

    [Fact]
    public void GetAllObservationsFromEmptyDatabase()
    {
        
    }

    [Fact]
    public void CreateSingleComment()
    {
        
    }

    [Fact]
    public void CreateMultipleComments()
    {
        
    }

    [Fact]
    public void GetSingleCommentFromObservation()
    {
        
    }

    [Fact]
    public void GetMultipleCommentsFromObservation()
    {
        
    }

    [Fact]
    public void GetCommentsFromNonExistingObservation()
    {
        
    }

    [Fact]
    public void CreateSingleCProposal()
    {
        
    }

    [Fact]
    public void CreateMultipleProposals()
    {
        
    }

    [Fact]
    public void GetSingleProposalFromObservation()
    {
        
    }

    [Fact]
    public void GetMultipleProposalsFromObservation()
    {
        
    }

    [Fact]
    public void GetProposalsFromNonExistingObservation()
    {
        
    }

    public void Dispose()
    {
        
    }
}