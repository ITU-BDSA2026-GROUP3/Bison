using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bison.Razor.Pages;

public class UserTimelineModel : PageModel
{
    private readonly IObservationService _observationService;

    public List<ObservationViewModel> Observations { get; private set; } = new();

    public UserTimelineModel(IObservationService observationService)
    {
        _observationService = observationService;
    }

    public string Author {get; private set; } = string.Empty;

    public int CurrentPage { get; private set; } = 1;

    public ActionResult OnGet([FromRoute] string author, [FromQuery] int page = 1)
    {
        Author = author;
        CurrentPage = Math.Max(page, 1);

        Observations = _observationService.GetObservationsFromAuthor(Author, CurrentPage);

        return Page();
    }
}
