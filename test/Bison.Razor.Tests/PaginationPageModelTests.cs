using Bison.Razor.Pages;

namespace Bison.Razor.Tests;

public class PaginationPageModelTests
{
    [Fact]
    public void PublicTimelinePassesRequestedPageToService()
    {
        FakeObservationService service = new();
        PublicModel model = new(service);

        model.OnGetAsync(2);

        Assert.Equal(2, model.CurrentPage);
        Assert.Equal(2, service.RequestedPublicPage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PublicTimelineNormalizesInvalidPageToOne(int page)
    {
        FakeObservationService service = new();
        PublicModel model = new(service);

        model.OnGetAsync(page);

        Assert.Equal(1, model.CurrentPage);
        Assert.Equal(1, service.RequestedPublicPage);
    }

    [Fact]
    public void UserTimelinePassesAuthorAndPageToService()
    {
        FakeObservationService service = new();
        UserTimelineModel model = new(service);

        model.OnGet(1, 2);

        Assert.Equal(1, model.AuthorId);
        Assert.Equal(2, model.CurrentPage);
        Assert.Equal(1, service.RequestedAuthor);
        Assert.Equal(2, service.RequestedAuthorPage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UserTimelineNormalizesInvalidPageToOne(int page)
    {
        FakeObservationService service = new();
        UserTimelineModel model = new(service);

        model.OnGet(1, page);

        Assert.Equal(1, model.CurrentPage);
        Assert.Equal(1, service.RequestedAuthorPage);
    }

    private class FakeObservationService : IObservationService
    {
        public int RequestedPublicPage { get; private set; }

        public int RequestedAuthor { get; private set; }

        public int RequestedAuthorPage { get; private set; }

        public Task<List<ObservationViewModel>> GetObservations(int page = 1)
        {
            RequestedPublicPage = page;

            return Task.FromResult(new List<ObservationViewModel>());
        }

        public Task<List<ObservationViewModel>> GetObservationsFromAuthor(int authorId, int page = 1)
        {
            RequestedAuthor = authorId;
            RequestedAuthorPage = page;

            return Task.FromResult(new List<ObservationViewModel>());
        }

        public Task<List<CommentViewModel>> GetComments(int observationId)
        {
            return Task.FromResult(new List<CommentViewModel>());
        }
    }
}