using Bison.Razor.domain_model;
using Bison.Razor.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bison.Razor.Pages.Authors;

public class SearchModel : PageModel
{
    private readonly IAuthorRepository _authorRepository;

    public SearchModel(IAuthorRepository authorRepository)
    {
        _authorRepository = authorRepository;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public List<Author> Authors { get; private set; } = new();

    public bool HasSearched { get; private set; }

    public async Task OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            return;
        }

        HasSearched = true;
        Authors = await _authorRepository.SearchAuthors(SearchTerm);
    }
}