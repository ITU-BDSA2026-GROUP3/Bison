using Bison.Razor.domain_model;
using Bison.Razor.DTOs;
using Bison.Razor.Pages;
using Bison.Razor.Repositories;

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
        UserTimelineModel model = new(service);

        await model.OnGet(1, 2);

        Assert.Equal(1, model.AuthorId);
        Assert.Equal(2, model.CurrentPage);
        Assert.Equal(1, service.RequestedAuthor);
        Assert.Equal(2, service.RequestedPage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task UserTimelineNormalizesInvalidPageToOne(int page)
    {
        FakeObservationService service = new();
        UserTimelineModel model = new(service);

        await model.OnGet(1, page);

        Assert.Equal(1, model.CurrentPage);
        Assert.Equal(1, service.RequestedPage);
    }

    private class FakeObservationService : IObservationService
    {
        public int RequestedPublicPage { get; private set; }

        public int RequestedAuthor { get; private set; }

        public int RequestedPage { get; private set; }
        public int RequestedTaxon { get; private set; }

        public Task<List<ObservationDto>> GetObservations(int page = 1)
        {
            RequestedPublicPage = page;

            return Task.FromResult(new List<ObservationDto>());
        }

        public Task<List<ObservationDto>> GetObservationsFromAuthor(int authorId, int page = 1)
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
    }

    private class FakeTaxonRepository : ITaxonRepository
    {
        List<Taxon> fakeTaxons = new List<Taxon> {new Taxon{Id = 0}};
        public FakeTaxonRepository()
        {
            
        }
        public async Task<List<int>> CreateTaxons(List<Taxon> taxons)
        {
            foreach(Taxon taxon in taxons)
            {
                fakeTaxons.Add(taxon);
            }
            return null;
        }

        public async Task<List<Taxon>> getAllTaxons()
        {
            await Task.Delay(0);
            return fakeTaxons;
        }

        public async Task<Taxon> getTaxon(int id)
        {
            await Task.Delay(0);
            return fakeTaxons[id];
        }
    }
}