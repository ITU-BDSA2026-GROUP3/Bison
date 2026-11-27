using Bison.Razor.domain_model;
using Bison.Razor.DTOs;
using Bison.Razor.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bison.Razor.Pages.Authors;

public class SearchModel : PageModel
{
    private readonly IObservationService _observationService;

    public SearchModel(IObservationService observationService)
    {
        _observationService = observationService;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public List<AuthorDto> Authors { get; private set; } = new();

    public List<ObservationDto> Observations { get; private set; } = new();
    public List<CommentDto> Comments { get; private set; } = new();
    public List<ProposalDto> Proposals { get; private set; } = new();

    public bool HasSearched { get; private set; }

    public async Task OnGetAsync()
    {
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            HasSearched = true;
            Authors = await _observationService.GetAuthorFromSearch(SearchTerm);
        }
    }
}