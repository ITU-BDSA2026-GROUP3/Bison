using Bison.Razor.domain_model;
using Bison.Razor.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bison.Razor.Pages.Authors;

public class SearchModel : PageModel
{
    private readonly IAuthorRepository _authorRepository;
    private readonly IPostRepository _postRepository;

    public SearchModel(
        IAuthorRepository authorRepository,
        IPostRepository postRepository)
    {
        _authorRepository = authorRepository;
        _postRepository = postRepository;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? AuthorId { get; set; }

    public List<Author> Authors { get; private set; } = new();

    public List<Post> Posts { get; private set; } = new();

    public bool HasSearched { get; private set; }

    public async Task OnGetAsync()
    {
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            HasSearched = true;
            Authors = await _authorRepository.SearchAuthors(SearchTerm);
        }

        if (!string.IsNullOrWhiteSpace(AuthorId))
        {
            Posts = await _postRepository.ReadPostsByAuthor(AuthorId);
        }
    }
}