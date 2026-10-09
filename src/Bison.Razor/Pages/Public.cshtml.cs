using Bison.Razor.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bison.Razor.Pages;

public class PublicModel : PageModel
{
    private const int PageSize = 32;
    private readonly IObservationService _observationService;
    public List<ObservationDto> Observations { get; private set; } = new();

    public PublicModel(IObservationService observationService)
    {
        _observationService = observationService;
    }

    public int CurrentPage {get; private set; } = 1;

    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => Observations.Count == PageSize;

    public async Task<ActionResult> OnGetAsync([FromQuery] int page = 1)
    {
        CurrentPage = Math.Max(page, 1);
        Observations = await _observationService.GetObservations(CurrentPage);
        return Page();
    }
}
