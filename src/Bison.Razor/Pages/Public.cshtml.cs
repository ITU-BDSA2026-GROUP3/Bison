using Bison.infrastructure.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bison.core.domain_model;
using Bison.infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
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
    public string NewObservationText{get; set;} = string.Empty;


    //the animal selected by user
    [BindProperty] //This attribute will connect the HTML form to these C# properties here
    [Required(ErrorMessage = "Please select an animal.")]
    public int? NewObservationTaxonId{get;set;}

    [BindProperty]
    public IFormFile? IMAGE { get; set; }

    [BindProperty]
    public String? LICENCE { get; set;}

    [BindProperty]
        public List<SelectListItem> Licences { get; } = new List<SelectListItem>
        {
            new() { Value = "CC BY", Text = "CC BY" },
            new() { Value = "CC BY-SA", Text = "CC BY-SA" },
            new() { Value = "CC BY-ND", Text = "CC BY-ND" },
            new() { Value = "CC BY-NC", Text = "CC BY-NC" },
            new() { Value = "CC BY-NC-SA", Text = "CC BY-NC-SA" },
            new() { Value = "CC BY-NC-ND", Text = "CC BY-NC-ND" }
        };

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

    public SelectList TaxonDropDown {get; private set;} 
    = new SelectList(new List<Taxon>(), "Id", "Name");

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
        if (IMAGE != null) {
            try {
                using var img = Image.Load(IMAGE.OpenReadStream());
                using var ms = new MemoryStream();
                if (img.Height > 720) img.Mutate(x => x.Resize(img.Width, 720));
                if (img.Width > 668) img.Mutate(x => x.Resize(668, img.Height));
                await img.SaveAsJpegAsync(ms);
                Byte[] bytes = ms.ToArray();
                await _observationService.CreateObservation(NewObservationText,authorID,NewObservationTaxonId.Value, bytes, LICENCE);
                return RedirectToPage("/Public");
            }
            catch
            {
                return BadRequest("File is not a valid image.");
            }
        }

        await _observationService.CreateObservation(NewObservationText,authorID,NewObservationTaxonId.Value);

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
