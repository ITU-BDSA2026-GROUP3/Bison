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

    public int AuthorId {get; private set; }

    public int CurrentPage { get; private set; } = 1;

    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => Observations.Count == PageSize;

    public async Task<ActionResult> OnGet([FromRoute] int authorId, [FromQuery] int page = 1)
    {
        AuthorId = authorId;
        CurrentPage = Math.Max(page, 1);

        Observations = await _observationService.GetObservationsFromAuthor(AuthorId, CurrentPage);

        return Page();
    }
}
