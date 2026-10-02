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
        _observationService = observationService;
    }

    public IActionResult OnGet(long? id)
    {
        // /ob/
        //This shows all observations so far.
        if(id == null)
        {
            Observations = _observationService.GetObservations();
            return Page();
        }

        // /ob/{id}
        // Show one specific observation.

        Observation = _observationService.GetObservation(id.Value);

        //The observation does not exist
        if(Observation == null)
        {
            return NotFound();
        }

        // Load comments belonging to this observation
        Comments = _observationService.GetComments(id.Value);
        
        return Page();

    }
}