using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bison.Razor.Pages;

public class PublicModel : PageModel
{
    private readonly IObservationService _observationService;
    public List<ObservationViewModel> Observations { get; private set; } = new();

    public PublicModel(IObservationService observationService)
    {
        _observationService = observationService;
    }

    public int CurrentPage {get; private set; } = 1;

    public ActionResult OnGet([FromQuery] int page = 1)
    {
        CurrentPage = Math.Max(page, 1);
        Observations = _observationService.GetObservations(CurrentPage);
        return Page();
    }
}
