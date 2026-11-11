using Bison.Razor.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bison.Razor.domain_model;
using Bison.Razor.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;


namespace Bison.Razor.Pages;

public class PublicModel : PageModel
{
    private const int PageSize = 32;
    private readonly IObservationService _observationService;
    private readonly ITaxonRepository _taxonRepository;
    public List<ObservationDto> Observations { get; private set; } = new();

    //The text that is entered by the user
    [BindProperty]
    [Required(ErrorMessage = "Please enter an observation")]
    public string newObservationText{get; set;} = string.Empty;


    //the animal selected by user
    [BindProperty] //This attribute will connect the HTML form to these C# properties here
    [Required(ErrorMessage = "Please select an animal.")]

    public int? NewObservationTaxonId{get;set;}

    public PublicModel(IObservationService observationService, ITaxonRepository taxonRepository)
    {
        _observationService = observationService;
        _taxonRepository = taxonRepository;
    }

    public int CurrentPage {get; private set; } = 1;

    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => Observations.Count == PageSize;
    [BindProperty(SupportsGet = true, Name = "taxonId")]
    public int TaxonFilter {get; set;} = 0;

    public SelectList TaxonDropDown {get; set;}

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if(User.Identity?.IsAuthenticated != true)
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
            await LoadPageDataAsync(1);
            return Page();
        }

        var taxons = await _taxonRepository.getAllTaxons();

        if(!NewObservationTaxonId.HasValue || !taxons.Any(t => t.Id == NewObservationTaxonId.Value))
        {
            ModelState.AddModelError(nameof(NewObservationTaxonId),
            "Please select a valid animal.");

            await LoadPageDataAsync(1);
            return Page();
        }

        await _observationService.CreateObservation(newObservationText,authorID,NewObservationTaxonId.Value);

        return RedirectToPage("/Public");
        
    }

    public async Task<ActionResult> OnGetAsync([FromQuery] int page = 1)
    {
        await LoadPageDataAsync(page);

        return Page();
       /* CurrentPage = Math.Max(page, 1);
        var taxons = await _taxonRepository.getAllTaxons();
        TaxonFilter = Math.Max(TaxonFilter, 0);
        TaxonDropDown = new SelectList(taxons, "Id", "Name", TaxonFilter);

        if(TaxonFilter != 0 && TaxonFilter < taxons.Count()+1) Observations = await _observationService.GetObservationsFromTaxon(TaxonFilter, CurrentPage);
        else Observations = await _observationService.GetObservations(CurrentPage);
        return Page();*/
    }

    private async Task LoadPageDataAsync(int page)
    {
        CurrentPage = Math.Max(page,1);

        var taxons = await _taxonRepository.getAllTaxons();

        TaxonFilter = Math.Max(TaxonFilter,0);

        if(TaxonFilter !=0 && !taxons.Any(t => t.Id == TaxonFilter))
        {
            TaxonFilter = 0;
        }

        TaxonDropDown = new SelectList(taxons, "Id", "Name", TaxonFilter);

        if(TaxonFilter != 0)
        {
            Observations = await _observationService.GetObservationsFromTaxon(TaxonFilter,CurrentPage);

        }
        else
        {
            Observations = await _observationService.GetObservations(CurrentPage);
        }
    }
}
