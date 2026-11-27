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

    [BindProperty(SupportsGet = true)]
    public string? AuthorId { get; set; }

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
            Console.WriteLine("Authors: "+Authors.Count);
        }

        if (!string.IsNullOrWhiteSpace(AuthorId))
        {
            Observations = await _observationService.GetObservationsFromAuthor(AuthorId);
            Comments = await _observationService.GetCommentsFromAuthor(AuthorId);
            Proposals = await _observationService.GetProposalsFromAuthor(AuthorId);
        }
    }
}