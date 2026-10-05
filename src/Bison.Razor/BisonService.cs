using Bison.Razor.Repositories;
using System.Data;
using static System.Net.Mime.MediaTypeNames;

public record ObservationViewModel(long obsID, string Author, string Message, string Location ,string Timestamp);
public record CommentViewModel(long obsID, string Comment);

public record ProposalViewModel(long obsID, string taxonID);


public interface IObservationService
{
    public Task<List<ObservationViewModel>> GetObservations(int page = 1);
    public Task<List<ObservationViewModel>> GetObservationsFromAuthor(int authorId, int page = 1);

    public Task<List<CommentViewModel>> GetComments(int observationId);

    Task<List<ProposalViewModel>> GetProposals(long id);
}

public class ObservationService : IObservationService
{

    private readonly IPostRepository _PostRepository;
    private readonly IAuthorRepository _AuthorRepository;
    private readonly ITaxonRepository _TaxonRepository;
    public ObservationService(IPostRepository postRepository, IAuthorRepository authorRepository, ITaxonRepository taxonRepository)
    {
        _PostRepository = postRepository;
        _AuthorRepository = authorRepository;
        _TaxonRepository = taxonRepository;
    }

    private const int PageSize = 32;

    private static readonly List<ObservationViewModel> _obs = new()
        {
            new ObservationViewModel(0, "Peter", "I saw a heron","Legoland", UnixTimeStampToDateTimeString(1690892208)),
            new ObservationViewModel(1, "Paul", "There is a bison on Amager","Amager", UnixTimeStampToDateTimeString(1690895308)),
        };
    private static readonly List<CommentViewModel> _comments = new()
        {
            new CommentViewModel(0,"OMG where?"),
            new CommentViewModel(1,"Nice observation bro!"),
        };


    public async Task<List<ObservationViewModel>> GetObservations(
        int page = 1)
    {
        int validPage = Math.Max(page, 1);

        List<ObservationViewModel> observations = new();

        var repoQueryResult =
            await _PostRepository.ReadAllObservations();

        foreach (var obs in repoQueryResult)
        {
            observations.Add(
                new ObservationViewModel(
                    obs.Id,
                    obs.AuthorName,
                    obs.Text,
                    "Jonas seems to have forgotten location",
                    obs.Timestamp));
        }

        return observations;
    }

    public async Task<List<ObservationViewModel>>
        GetObservationsFromAuthor(
            int authorId,
            int page = 1)
    {
        int validPage = Math.Max(page, 1);

        List<ObservationViewModel> observations = new();

        var repoQueryResult =
            await _PostRepository.ReadObservations(authorId);

        foreach (var obs in repoQueryResult)
        {
            observations.Add(
                new ObservationViewModel(
                    obs.Id,
                    obs.AuthorName,
                    obs.Text,
                    "Jonas seems to have forgotten location",
                    obs.Timestamp));
        }

        return observations;
    }
    public async Task<List<CommentViewModel>> GetComments(
        int observationId)
    {
        List<CommentViewModel> comments = new();

        var repoQueryResult =
            await _PostRepository.ReadComments(observationId);

        foreach (var comment in repoQueryResult)
        {
            comments.Add(
                new CommentViewModel(
                    comment.Id,
                    comment.Text));
        }

        return comments;
    }

    private static string UnixTimeStampToDateTimeString(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime.ToString("MM/dd/yy H:mm:ss");
    }

    public async Task<List<ProposalViewModel>> GetProposals(long id)
    {
        List<ProposalViewModel> proposals = new();

        var repoQueryResult =
            await _PostRepository.ReadProposals((int)id);

        foreach (var proposal in repoQueryResult)
        {
            proposals.Add(
                new ProposalViewModel(
                    proposal.ObservationId,
                    proposal.TaxonName));
        }

        return proposals;
    }
}
