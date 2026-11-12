using Bison.Razor.domain_model;
using Bison.Razor.DTOs;
using Bison.Razor.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Xunit;

namespace Bison.Razor.Tests;


public class ObservationServiceTests
{
    [Fact]
    public async Task CreateObservation_ValidData_SavesObservation()
    {
        var author = new Author {Id = "author-1"};
        var taxon = new Taxon {Id =10, Name = "Bison"};

        var postRepository = new FakePostRepository();
        var authorRepository = new FakeAuthorRepository(author);
        var taxonRepository = new FakeTaxonRepository(taxon);

        var service = new ObservationService(postRepository,authorRepository,taxonRepository);

        var result = await service.CreateObservation("I saw a bison!",author.Id,taxon.Id);

        
        var savedObservation = Assert.Single(postRepository.CreatedObservations);

        Assert.Equal("I saw a bison!", savedObservation.Text);
        Assert.Same(author,savedObservation.Author);
        Assert.Same(taxon, savedObservation.Taxon);
        Assert.NotEqual(default(DateTime), savedObservation.TimeStamp);

        //result
        Assert.Single(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]

    public async Task CreateObservation_EmptyText_ThrowsException(string text)
    {
        var author = new Author {Id = "author-1"};
        var taxon = new Taxon {Id = 10, Name ="Bison"};

        var postRepository = new FakePostRepository();

        var service = new ObservationService(postRepository,new FakeAuthorRepository(author), new FakeTaxonRepository(taxon));

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateObservation(text, author.Id, taxon.Id));

            //Nothing should have been saved
            Assert.Empty(postRepository.CreatedObservations);   
        }

        [Fact]
        public async Task CreateObservation_InvalidAuthor_ThrowsException(){
            var taxon = new Taxon {Id =10, Name = "Bison"};

            var postRepository = new FakePostRepository();

            var service = new ObservationService(postRepository, 
            new FakeAuthorRepository(null), 
            new FakeTaxonRepository(taxon));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateObservation("I saw a bison!","missing-author",taxon.Id));

                Assert.Empty(postRepository.CreatedObservations);

    }

     [Fact]
     public async  Task CreateObservation_InvalidTaxon_ThrowsException()
        {
            var author = new Author{Id = "author-1"};

            var postRepository = new FakePostRepository();

            var service = new ObservationService(postRepository, new FakeAuthorRepository(author),new FakeTaxonRepository(null));

            await Assert.ThrowsAsync<InvalidOperationException>( () => service.CreateObservation("I saw a bison!", author.Id,999));

            Assert.Empty(postRepository.CreatedObservations);

           
        }

        private class FakeAuthorRepository :IAuthorRepository
    {
        private readonly Author? _author;

        public FakeAuthorRepository(Author? author)
        {
            _author = author;
        }

        public Task<Author?> getAuthor(string id)
        {
            return Task.FromResult<Author?>(
                _author?.Id == id ? _author : null);
            
        }

        public Task<List<string>> CreateAuthors(List<Author> authors)
        {
            return Task.FromResult(new List<string>());
        }
    }

        //Fake TaxonRepo
        private class FakeTaxonRepository : ITaxonRepository
        {
            private readonly Taxon? _taxon;

            public FakeTaxonRepository(Taxon? taxon)
            {
                _taxon = taxon;
            }

            public Task<Taxon?> getTaxon(int id)
            {
                return Task.FromResult<Taxon?>(
                    _taxon?.Id == id ? _taxon : null);                
            }

            public Task<List<Taxon>> getAllTaxons()
            {
                var taxons = _taxon == null
                ? new List<Taxon>()
                : new List<Taxon> {_taxon};
                
                return Task.FromResult(taxons); 
        
            }
            public Task<List<int>> CreateTaxons(List<Taxon> taxons)
                {
                    return Task.FromResult(new List<int>());
                }    
    }


//Fake repository for testing, for post
private class FakePostRepository : IPostRepository
    {

        public List<Observation> CreatedObservations{get;} = new();
        public Task<List<int>> CreateObservations(List<Observation> observations)
        {
            CreatedObservations.AddRange(observations);

            return Task.FromResult(Enumerable.Range(1,observations.Count).ToList());
        }
        public Task<Observation?> ReadSingleObservation(int observationID)
        {
            return Task.FromResult<Observation?>(null);
        }

    public Task<List<Observation>> ReadObservations(string authorID)
        {
            return Task.FromResult(new List<Observation>());
        }

        public Task<List<Observation>> ReadAllObservations()
        {
            return Task.FromResult(new List<Observation>());
            
        }
        
    
public Task<List<int>> CreateComments(List<Comment> comments)
{
    return Task.FromResult(new List<int>());

}

public Task<List<Comment>> ReadComments(int obsID)
    {
        return Task.FromResult(new List<Comment>());
    }

    public Task<List<int>> CreateProposals(List<Proposal> proposals)
    {
        return Task.FromResult(new List<int>());
    }

    public Task<List<Proposal>> ReadProposals(int obsID)
    {
        return Task.FromResult(new List<Proposal>());
    }

    }
}