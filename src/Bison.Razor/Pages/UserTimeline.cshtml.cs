using Bison.Razor.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Bison.Razor.Repositories;
using Bison.Razor.domain_model;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;


namespace Bison.Razor.Pages;

[Authorize]
public class UserTimelineModel : PageModel
{
    private const int PageSize = 32;
    private readonly IObservationService _observationService;

    public List<ObservationDto> Observations { get; private set; } = new();

    public UserTimelineModel(IObservationService observationService,ITaxonRepository taxonRepository)


       { _observationService = observationService;
        _taxonRepository = taxonRepository;
        }

    public async Task<IActionResult> OnPostCreateAsync(
        [FromRoute] string authorId)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(currentUserId))
        {
            return Challenge();
        }

        if(currentUserId != authorId)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            await LoadPageDataAsync(authorId, 1);
            return Page();
        }

        var taxons = await _taxonRepository.getAllTaxons();

        if(!NewObservationTaxonId.HasValue || !taxons.Any(t => t.Id == NewObservationTaxonId.Value))
        {
            ModelState.AddModelError(nameof(NewObservationTaxonId),"Please select as valid animal.");

            await LoadPageDataAsync(authorId,1);
            return Page();
        }

        await _observationService.CreateObservation(NewObservationText,currentUserId,NewObservationTaxonId.Value);

        return RedirectToPage("/UserTimeline", new {authorId = currentUserId});
    }

    public AuthorDto? author
    {
        get; private set;
    }

    public int CurrentPage { get; private set; } = 1;

    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => Observations.Count == PageSize;

    private readonly ITaxonRepository _taxonRepository;

    [BindProperty]
    [Required(ErrorMessage = "Please enter an observation.")]
    public string NewObservationText{get; set;} = string.Empty;

    [BindProperty]
    [Required(ErrorMessage ="Please select an animal.")]
    public int? NewObservationTaxonId{get; set;}

    public SelectList TaxonDropDown{get; private set;}
    = new SelectList(new List <Taxon>(), "Id", "Name");

    public async Task<ActionResult> OnGetAsync([FromRoute] string authorId, [FromQuery] int page = 1)
    {
    
        await LoadPageDataAsync(authorId, page);

        return Page();
    }

    private async Task LoadPageDataAsync(string authorId, int page)
    {
        author = await _observationService.GetAuthor(authorId);

        CurrentPage = Math.Max(page,1);

        if(author != null)
        {
            Observations = await _observationService.GetObservationsFromAuthor(authorId,CurrentPage);
        }
        else
        {
            Observations = new List<ObservationDto>();

            
        }

        var taxons = await _taxonRepository.getAllTaxons();

        TaxonDropDown = new SelectList(taxons, "Id","Name", NewObservationTaxonId);
    }
    
}