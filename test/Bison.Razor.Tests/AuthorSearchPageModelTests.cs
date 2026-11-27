using Bison.Razor.domain_model;
using Bison.Razor.DTOs;
using Bison.Razor.Pages.Authors;
using Bison.Razor.Repositories;

namespace Bison.Razor.Tests;

public class AuthorSearchPageModelTests
{
    [Fact]
    public async Task AuthorSearchReturnsMatchingAuthors()
    {
        FakeObservationService fakeObservationService = new FakeObservationService();
        SearchModel model = new(fakeObservationService)
        {
            SearchTerm = "Emil"
        };

        await model.OnGetAsync();

        Assert.True(model.HasSearched);
        Assert.Single(model.Authors);
        Assert.Equal("Emil", model.Authors[0].Name);
        Assert.Equal("Emil", fakeObservationService.ReceivedSearchTerm);
    }

    [Fact]
    public async Task EmptySearchDoesNotSearchRepository()
    {
        FakeObservationService fakeObservationService = new FakeObservationService();
        SearchModel model = new(fakeObservationService)
        {
            SearchTerm = " "
        };

        await model.OnGetAsync();

        Assert.False(model.HasSearched);
        Assert.Empty(model.Authors);
        Assert.Null(fakeObservationService.ReceivedSearchTerm);
    }

    private class FakeObservationService : IObservationService
    {
        public string ReceivedSearchTerm;
        List<AuthorDto> authors =
            [
                new AuthorDto
                {
                    Id = "author-1",
                    Name = "Emil"
                }
            ];
        
        public Task<List<int>> CreateObservation(
            string text, string authorID, int taxonID)
        {
            return Task.FromResult(new List<int>());
            
        }

        public Task<List<int>> CreateComment(
            string text,
            string authorID,
            int observationID)
        {
            return Task.FromResult(new List<int>());
        }

        public Task<List<int>> CreateProposal(
            string text,
            string authorID,
            int observationID,
            int taxonID)
        {
            return Task.FromResult(new List<int>());
        }
        

        public int RequestedPublicPage { get; private set; }

        public string RequestedAuthor { get; private set; }

        public int RequestedPage { get; private set; }
        public int RequestedTaxon { get; private set; }

        public Task<List<ObservationDto>> GetObservations(int page = 1)
        {
            RequestedPublicPage = page;

            return Task.FromResult(new List<ObservationDto>());
        }

        public Task<List<ObservationDto>> GetObservationsFromAuthor(string authorId, int page = 1)
        {
            RequestedAuthor = authorId;
            RequestedPage = page;

            return Task.FromResult(new List<ObservationDto>());
        }

        public Task<List<CommentDto>> GetComments(int observationId)
        {
            return Task.FromResult(new List<CommentDto>());
        }

        public Task<List<ObservationDto>> GetObservationsFromTaxon(int taxonId, int page = 1)
        {
            RequestedTaxon = taxonId;
            RequestedPage = page;

            return Task.FromResult(new List<ObservationDto>());
        }

        public Task<ObservationDto> GetObservationFromId(int observationId, int page = 1)
        {
            throw new NotImplementedException();
        }

        public Task<List<ProposalDto>> GetProposals(int observationId)
        {
            throw new NotImplementedException();
        }

        public Task<AuthorDto?> GetAuthor(string authorId)
        {
            return Task.FromResult(new AuthorDto {Id = authorId});
        }

        public Task ForgetUser(string authorId, bool keepObservations)
        {
            return Task.CompletedTask;
        }

        public Task<List<int>> CreateObservation(string text, string authorID, int taxonID, byte[] imgbytes)
        {
            throw new NotImplementedException();
        }

        public Task<List<CommentDto>> GetCommentsFromAuthor(string authorId)
        {
            throw new NotImplementedException();
        }

        public Task<List<ProposalDto>> GetProposalsFromAuthor(string authorId)
        {
            throw new NotImplementedException();
        }

        public Task<List<AuthorDto?>> GetAuthorsFromSearch(string search)
        {
            ReceivedSearchTerm = search;
            return Task.FromResult(authors);
        }
    }
}