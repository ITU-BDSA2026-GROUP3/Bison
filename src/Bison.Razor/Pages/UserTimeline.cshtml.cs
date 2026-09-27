using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bison.Razor.Pages;

public class UserTimelineModel : PageModel
{
    private const int PageSize = 32;
    private readonly IObservationService _observationService;

    public List<ObservationViewModel> Observations { get; private set; } = new();

    public UserTimelineModel(IObservationService observationService)
    {
        _observationService = observationService;
    }

    public string Author {get; private set; } = string.Empty;

    public int CurrentPage { get; private set; } = 1;

    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => Observations.Count == PageSize;

    public ActionResult OnGet([FromRoute] string author, [FromQuery] int page = 1)
    {
        Author = author;
        CurrentPage = Math.Max(page, 1);

        Observations = _observationService.GetObservationsFromAuthor(Author, CurrentPage);

        return Page();
    }
}
