using Bison.Razor.domain_model;
using Bison.Razor.Pages.Authors;
using Bison.Razor.Repositories;

namespace Bison.Razor.Tests;

public class AuthorSearchPageModelTests
{
    [Fact]
    public async Task AuthorSearchReturnsMatchingAuthors()
    {
        FakeAuthorRepository authorRepository = new();
        FakePostRepository postRepository = new();
        SearchModel model = new(authorRepository, postRepository)
        {
            SearchTerm = "Emil"
        };

        await model.OnGetAsync();

        Assert.True(model.HasSearched);
        Assert.Single(model.Authors);
        Assert.Equal("Emil", model.Authors[0].UserName);
        Assert.Equal("Emil", authorRepository.ReceivedSearchTerm);
    }

    [Fact]
    public async Task EmptySearchDoesNotSearchRepository()
    {
        FakeAuthorRepository authorRepository = new();
        FakePostRepository postRepository = new();
        SearchModel model = new(authorRepository, postRepository)
        {
            SearchTerm = " "
        };

        await model.OnGetAsync();

        Assert.False(model.HasSearched);
        Assert.Empty(model.Authors);
        Assert.Null(authorRepository.ReceivedSearchTerm);
    }

    [Fact]
    public async Task SelectedAuthorReturnsPostHistory()
    {
        FakeAuthorRepository authorRepository = new();
        FakePostRepository postRepository = new();
        SearchModel model = new(authorRepository, postRepository)
        {
            AuthorId = "author-1"
        };

        await model.OnGetAsync();

        Assert.Single(model.Posts);
        Assert.Equal("Test observation", model.Posts[0].Text);
        Assert.Equal("author-1", postRepository.ReceivedAuthorId);
    }

    private class FakeAuthorRepository : IAuthorRepository
    {
        public string? ReceivedSearchTerm { get; private set; }

        public Task<List<Author>> SearchAuthors(string searchTerm)
        {
            ReceivedSearchTerm = searchTerm;

            List<Author> authors =
            [
                new Author
                {
                    Id = "author-1",
                    UserName = "Emil"
                }
            ];

            return Task.FromResult(authors);
        }

        public Task<List<string>> CreateAuthors(List<Author> authors)
        {
            return Task.FromResult(new List<string>());
        }

        public Task<Author?> getAuthor(string authorID)
        {
            return Task.FromResult<Author?>(null);
        }

        public Task AnonymizeAuthor(string authorID)
        {
            return Task.CompletedTask;
        }
    }

    private class FakePostRepository : IPostRepository
    {
        public string? ReceivedAuthorId { get; private set; }

        public Task<List<Post>> ReadPostsByAuthor(string authorID)
        {
            ReceivedAuthorId = authorID;

            List<Post> posts =
            [
                new Observation
                {
                    Text = "Test observation"
                }
            ];

            return Task.FromResult(posts);
        }

        public Task<List<int>> CreateObservations(
            List<Observation> observations)
        {
            return Task.FromResult(new List<int>());
        }

        public Task<Observation?> ReadSingleObservation(int observationID)
        {
            return Task.FromResult<Observation?>(null);
        }

        public Task<List<Observation>> ReadObservations(string authorID)
        {
            return Task.FromResult(new List<Observation>());
        }

        public Task<List<Observation>> ReadAllObservations()
        {
            return Task.FromResult(new List<Observation>());
        }

        public Task HandleForgottenUserObservations(
            string authorID,
            bool keepObservations)
        {
            return Task.CompletedTask;
        }

        public Task<List<int>> CreateComments(List<Comment> comments)
        {
            return Task.FromResult(new List<int>());
        }

        public Task<List<Comment>> ReadComments(int obsID)
        {
            return Task.FromResult(new List<Comment>());
        }

        public Task<List<int>> CreateProposals(List<Proposal> proposals)
        {
            return Task.FromResult(new List<int>());
        }

        public Task<List<Proposal>> ReadProposals(int obsID)
        {
            return Task.FromResult(new List<Proposal>());
        }
    }
}