using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bison.Razor.Pages;


public class ObModel : PageModel

{
    private readonly IObservationService _ObservationService;

    public ObservationViewModel? Observation {get; private set;}

    public List<ObservationViewModel> Observations {get; private set; } = new();

    public List<CommentViewModel> Comments { get; private set;} = new();

    public ObModel(IObservationService observationService)
    {
        _ObservationService = observationService;
    }

    public IActionResult OnGet(long? id)
    {
        // /ob/
        //This shows all observations so far.
        if(id == null)
        {
            Observations = _ObservationService.GetObservations();
            return Page();
        }

    }
}