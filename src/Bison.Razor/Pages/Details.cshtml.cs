using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Bison.Razor.DTOs;
using Bison.Razor.domain_model;
using Bison.Razor.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bison.Razor.Pages;

public class DetailsModel : PageModel
{
    private readonly IObservationService _observationService;
    private readonly ITaxonRepository _taxonRepository;


    public ObservationDto? Observation{ get; private set; }
    public List<CommentDto> Comments { get; set; } = new();
    public List<ProposalDto> Proposals { get; set; } = new();
    public SelectList TaxonDropDown { get; private set; } = new SelectList(new List<Taxon>(), "Id", "Name");


    [BindProperty]
    [Required(ErrorMessage = "The comment cannot be empty.")]
    public string CommentText { get; set; } = "";

    public DetailsModel(IObservationService service, ITaxonRepository taxonRepository)
    {
        _observationService = service;
        _taxonRepository = taxonRepository;
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

        var taxons = await _taxonRepository.getAllTaxons();
        TaxonDropDown = new SelectList(taxons, "Id", "Name");

        return true;
    }

    public async Task<IActionResult> OnPostProposalAsync([FromRoute] int id, string proposalText, int? proposalTaxonId)
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
        if (string.IsNullOrWhiteSpace(proposalText))
        {
            ModelState.AddModelError(nameof(proposalText), "The proposal cannot be empty.");
        }
        var taxons = await _taxonRepository.getAllTaxons();
        if (!proposalTaxonId.HasValue ||
            !taxons.Any(taxon => taxon.Id == proposalTaxonId.Value))
        {
            ModelState.AddModelError(nameof(proposalTaxonId), "Please select a valid animal.");
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
        await _observationService.CreateProposal(
            proposalText,
            authorID,
            id,
            proposalTaxonId!.Value);
        return RedirectToPage("/Details", new { id });
    }
}
