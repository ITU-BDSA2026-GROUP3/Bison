using Bison.Razor.domain_model;
using Bison.Razor.DTOs;
using Bison.Razor.Pages;
using Bison.Razor.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bison.Razor.Tests;

public class PaginationPageModelTests
{
    [Fact]
    public async Task PublicTimelinePassesRequestedPageToService()
    {
        FakeObservationService service = new();
        FakeTaxonRepository repository = new();
        PublicModel model = new(service, repository);

        await model.OnGetAsync(2);

        Assert.Equal(2, model.CurrentPage);
        Assert.Equal(2, service.RequestedPublicPage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task PublicTimelineNormalizesInvalidPageToOne(int page)
    {
        FakeObservationService service = new();
        FakeTaxonRepository repository = new();
        PublicModel model = new(service, repository);

        await model.OnGetAsync(page);

        Assert.Equal(1, model.CurrentPage);
        Assert.Equal(1, service.RequestedPublicPage);
    }

    [Fact]
    public async Task UserTimelinePassesAuthorAndPageToService()
    {
        FakeObservationService service = new();
        FakeTaxonRepository taxonRepository = new();
        UserTimelineModel model = new(service, taxonRepository);

        await model.OnGetAsync("1", 2);

        Assert.Equal("1", model.author.Id);
        Assert.Equal(2, model.CurrentPage);
        Assert.Equal("1", service.RequestedAuthor);
        Assert.Equal(2, service.RequestedPage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task UserTimelineNormalizesInvalidPageToOne(int page)
    {
        FakeObservationService service = new();
        FakeTaxonRepository taxonRepository = new();
        UserTimelineModel model = new(service, taxonRepository);

        await model.OnGetAsync("1", page);

        Assert.Equal(1, model.CurrentPage);
        Assert.Equal(1, service.RequestedPage);
    }

    private class FakeObservationService : IObservationService
    {
        public Task<List<int>> CreateObservation(
            string text, string authorID, int taxonID)
        {
            return Task.FromResult(new List<int>());
            
        }

        public Task<List<int>> CreateComment(
            string text,
            string authorID,
            int observationID)
        {
            return Task.FromResult(new List<int>());
        }
        

        public int RequestedPublicPage { get; private set; }

        public string RequestedAuthor { get; private set; }

        public int RequestedPage { get; private set; }
        public int RequestedTaxon { get; private set; }

        public Task<List<ObservationDto>> GetObservations(int page = 1)
        {
            RequestedPublicPage = page;

            return Task.FromResult(new List<ObservationDto>());
        }

        public Task<List<ObservationDto>> GetObservationsFromAuthor(string authorId, int page = 1)
        {
            RequestedAuthor = authorId;
            RequestedPage = page;

            return Task.FromResult(new List<ObservationDto>());
        }

        public Task<List<CommentDto>> GetComments(int observationId)
        {
            return Task.FromResult(new List<CommentDto>());
        }

        public Task<List<ObservationDto>> GetObservationsFromTaxon(int taxonId, int page = 1)
        {
            RequestedTaxon = taxonId;
            RequestedPage = page;

            return Task.FromResult(new List<ObservationDto>());
        }

        public Task<ObservationDto> GetObservationFromId(int observationId, int page = 1)
        {
            throw new NotImplementedException();
        }

        public Task<List<ProposalDto>> GetProposals(int observationId)
        {
            throw new NotImplementedException();
        }

        public Task<AuthorDto?> GetAuthor(string authorId)
        {
            return Task.FromResult(new AuthorDto {Id = authorId});
        }
    }

    private class FakeTaxonRepository : ITaxonRepository
    {
        List<Taxon> fakeTaxons = new List<Taxon> {new Taxon{Id = 0}};
        public FakeTaxonRepository()
        {
            
        }
        public Task<List<int>> CreateTaxons(List<Taxon> taxons)
        {
            foreach(Taxon taxon in taxons)
            {
                fakeTaxons.Add(taxon);
            }
            return Task.FromResult(new List<int>());
        }

        public Task<List<Taxon>> getAllTaxons()
        {
            return Task.FromResult(fakeTaxons);
        }

        public Task<Taxon?> getTaxon(int id)
        {
            var taxon = fakeTaxons.FirstOrDefault(o => o.Id == id);

            return Task.FromResult(taxon);
        }
    }

    private class FakeAuthorRepository : IAuthorRepository
    {
        List<Author> fakeAuthors = new List<Author> {new Author{Id = "1" }};
        public FakeAuthorRepository()
        {
            
        }
        public Task<List<string>> CreateAuthors(List<Author> authors)
        {
            foreach(Author author in authors)
            {
                fakeAuthors.Add(author);
            }
            return Task.FromResult(new List<string>());
        }

        public Task<Author?> getAuthor(string id)
        {
            var author = fakeAuthors.FirstOrDefault(o => o.Id == id);

            return Task.FromResult(author);
        }
    }
}