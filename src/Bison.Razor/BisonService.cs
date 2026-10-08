using Bison.Razor.domain_model;
using Bison.Razor.Repositories;
using System.Data;
using static System.Net.Mime.MediaTypeNames;

public record ObservationViewModel(long obsID, string Author, string Message, string Location ,string Timestamp);
public record CommentViewModel(long obsID, string Comment);
public interface IObservationService
{
    public Task<List<ObservationViewModel>> GetObservations(int page = 1);
    public Task<List<ObservationViewModel>> GetObservationsFromAuthor(int authorId, int page = 1);
    public Task<List<ObservationViewModel>> GetObservationsFromTaxon(int authorId, int page = 1);

    public Task<List<CommentViewModel>> GetComments(int observationId);
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
    // These would normally be loaded from a database for example
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


    public async Task<List<ObservationViewModel>> GetObservations(int page = 1)
    {

        int validPage = Math.Max(page, 1);
        List<ObservationViewModel> observations = new List<ObservationViewModel>();
        var repoQueryResult = await _PostRepository.ReadAllObservations();
        foreach (var obs in repoQueryResult)
        {
            observations.Add(new ObservationViewModel(
                obs.Id,
                obs.AuthorName,
                obs.Text,
                "Jonas seems to have forgotten location",
                obs.Timestamp));
        }
    
        return observations;
    }

    public async Task<List<ObservationViewModel>> GetObservationsFromAuthor(int authorId, int page = 1)
    {
        int validPage = Math.Max(page, 1);
        List<ObservationViewModel> observations = new List<ObservationViewModel>();
        var repoQueryResult = await _PostRepository.ReadObservations(authorId);
        foreach (var obs in repoQueryResult)
        {
            observations.Add(new ObservationViewModel(
                obs.Id,
                obs.AuthorName,
                obs.Text,
                "Jonas seems to have forgotten location",
                obs.Timestamp));
        }

        return observations;
    }

    public async Task<List<ObservationViewModel>> GetObservationsFromTaxon(int taxonId, int page = 1)
    {
        int validPage = Math.Max(page, 1);
        Taxon taxon = await _TaxonRepository.getTaxon(taxonId);
        var obs = await _PostRepository.ReadAllObservationObjects();
        Observation[] observations = obs.ToArray();
        var filt = filterChecked.__default.FilterByChecked(taxon, Dafny.Sequence<Observation>.FromArray(observations));
        List<ObservationViewModel> filteredObservations = new List<ObservationViewModel>();
        foreach(var ob in filt)
        {
           filteredObservations.Add(new ObservationViewModel(
            ob.Id,
            ob.Author.Name,
            ob.Text,
            "Jonas seems to have forgotten location",
            ob.TimeStamp.ToString("MM/dd/yy H:mm:ss")
           )) ;
        }
        return filteredObservations;
    }

    public async Task<List<CommentViewModel>> GetComments(int observationId)
    {
        List<CommentViewModel> comments = new List<CommentViewModel>();

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

}
