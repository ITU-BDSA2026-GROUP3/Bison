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

        model.OnGet("Peter", 2);

        Assert.Equal("Peter", model.Author);
        Assert.Equal(2, model.CurrentPage);
        Assert.Equal("Peter", service.RequestedAuthor);
        Assert.Equal(2, service.RequestedAuthorPage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UserTimelineNormalizesInvalidPageToOne(int page)
    {
        FakeObservationService service = new();
        UserTimelineModel model = new(service);

        model.OnGet("Peter", page);

        Assert.Equal(1, model.CurrentPage);
        Assert.Equal(1, service.RequestedAuthorPage);
    }

    private class FakeObservationService : IObservationService
    {
        public int RequestedPublicPage { get; private set; }

        public string RequestedAuthor { get; private set; } = string.Empty;

        public int RequestedAuthorPage { get; private set; }

        public List<ObservationViewModel> GetObservations(int page = 1)
        {
            RequestedPublicPage = page;
            return new List<ObservationViewModel>();
        }

        public List<ObservationViewModel> GetObservationsFromAuthor(
            string author,
            int page = 1)
        {
            RequestedAuthor = author;
            RequestedAuthorPage = page;

            return new List<ObservationViewModel>();
        }

        public List<CommentViewModel> GetComments(long id)
        {
            return new List<CommentViewModel>();
        }
    }
}