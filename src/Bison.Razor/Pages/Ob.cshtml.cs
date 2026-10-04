using System.Security.Cryptography.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Net.Http.Headers;

namespace Bison.Razor.Pages;


public class ObModel : PageModel

{
    private readonly IObservationService _observationService;

    public ObservationViewModel? Observation {get; private set;}

    public List<ObservationViewModel> Observations {get; private set; } = new();

    public List<CommentViewModel> Comments {get; private set; } = new();

    public List<ProposalViewModel> Proposals {get; private set;} = new();

    public ObModel(IObservationService observationService)
    {
        _observationService = observationService;
    }

    

    public IActionResult OnGet([FromRoute]long? id,[FromQuery] int page = 1)
    {
        // /ob/
        //This shows all observations so far.
        if(id == null)
        {
            Observations = _observationService.GetObservations(page);
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

        // Show the comments and proposals for this obseration
        Comments = _observationService.GetComments(id.Value);
        Proposals = _observationService.GetProposals(id.Value);
       
        return Page();

    }
}