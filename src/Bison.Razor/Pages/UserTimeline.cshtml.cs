using Bison.Razor.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Bison.Razor.Repositories;

namespace Bison.Razor.Pages;

[Authorize]
public class UserTimelineModel : PageModel
{
    private const int PageSize = 32;
    private readonly IObservationService _observationService;

    public List<ObservationDto> Observations { get; private set; } = new();

    public UserTimelineModel(IObservationService observationService)
    {
        _observationService = observationService;
    }

    public AuthorDto? author
    {
        get; private set;
    }

    public int CurrentPage { get; private set; } = 1;

    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => Observations.Count == PageSize;

    public async Task<ActionResult> OnGet([FromRoute] string authorId, [FromQuery] int page = 1)
    {
        author = await _observationService.GetAuthor(authorId);
        if (author != null)
        {
            CurrentPage = Math.Max(page, 1);

            Observations = await _observationService.GetObservationsFromAuthor(authorId, CurrentPage);
        }

        return Page();
    }
}