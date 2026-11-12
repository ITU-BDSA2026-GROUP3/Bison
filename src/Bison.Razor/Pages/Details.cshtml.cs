using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
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

    [BindProperty]
    [Required(ErrorMessage = "The comment cannot be empty.")]
    public string CommentText { get; set; } = "";

    public DetailsModel(IObservationService service)
    {
        _observationService = service;
    }

    public async Task<IActionResult> OnGet([FromRoute] int id, [FromQuery] int page = 1)
    {
        var observationExists = await LoadPageDataAsync(id);
        if (!observationExists)
        {
            return NotFound();
        }
        return Page();
    }

    public async Task<IActionResult> OnPostCommentAsync([FromRoute] int id)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Challenge();
        }
        var authorID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(authorID))
        {
            return Challenge();
        }
        if (!ModelState.IsValid)
        {
            var observationExists = await LoadPageDataAsync(id);
            if (!observationExists)
            {
                return NotFound();
            }
            return Page();
        }
        await _observationService.CreateComment(
            CommentText,
            authorID,
            id);
        return RedirectToPage("/Details", new { id });
    }

    private async Task<bool> LoadPageDataAsync(int id)
    {
        Observation = await _observationService.GetObservationFromId(id);
        if (Observation == null)
        {
            return false;
        }
        Comments = await _observationService.GetComments(id);
        Proposals = await _observationService.GetProposals(id);
        return true;
    }

}
