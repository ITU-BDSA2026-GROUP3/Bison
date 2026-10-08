using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bison.Razor.domain_model;
using Bison.Razor.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bison.Razor.Pages;

public class PublicModel : PageModel
{
    private const int PageSize = 32;
    private readonly IObservationService _observationService;
    private readonly ITaxonRepository _taxonRepository;
    public List<ObservationViewModel> Observations { get; private set; } = new();

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

    public required SelectList TaxonDropDown {get; set;}

    public async Task<ActionResult> OnGetAsync([FromQuery] int page = 1)
    {
        CurrentPage = Math.Max(page, 1);
        var taxons = await _taxonRepository.getAllTaxons();
        TaxonFilter = Math.Max(TaxonFilter, 0);
        TaxonDropDown = new SelectList(taxons, "Id", "Name", TaxonFilter);

        if(TaxonFilter != 0 && TaxonFilter < taxons.Count()+1) Observations = await _observationService.GetObservationsFromTaxon(TaxonFilter, CurrentPage);
        else Observations = await _observationService.GetObservations(CurrentPage);
        return Page();
    }
}
