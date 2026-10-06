using Bison.Razor.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bison.Razor.Pages;

public class DetailsModel : PageModel
{
    private readonly IObservationService _observationService;

    public ObservationDto? Observation{ get; private set; }
    public List<CommentDto> Comments { get; set; } = new();
    public List<ProposalDto> Proposals { get; set; } = new();

    public DetailsModel(IObservationService service)
    {
        _observationService = service;
    }

    public async Task<IActionResult> OnGet([FromRoute] int id, [FromQuery] int page = 1)
    {

        // /ob/{id}
        // Show one specific observation.
        Observation = await _observationService.GetObservationFromId(id);

        //The observation does not exist
        if (Observation == null)
        {
            return NotFound();
        }

        // Show the comments and proposals for this obseration
        Comments = await _observationService.GetComments(id);
        //Proposals = _observationService.GetProposals(id);

        return Page();

    }
}
