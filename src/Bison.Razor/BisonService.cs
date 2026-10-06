using Bison.Razor.DTOs;
using Bison.Razor.Repositories;
using System.Data;
using static System.Net.Mime.MediaTypeNames;

//public record ObservationViewModel(long obsID, string Author, string Message, string Location ,string Timestamp);
//public record CommentViewModel(long obsID, string Comment);
public interface IObservationService
{
    public Task<List<ObservationDto>> GetObservations(int page = 1);
    public Task<List<ObservationDto>> GetObservationsFromAuthor(int authorId, int page = 1);

    public Task<ObservationDto> GetObservationFromId(int observationId, int page = 1);

    public Task<List<CommentDto>> GetComments(int observationId);
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
   

    public async Task<List<ObservationDto>> GetObservations(int page = 1)
    {

        int validPage = Math.Max(page, 1);
        List<ObservationDto> observations = new List<ObservationDto>();
        var repoQueryResult = await _PostRepository.ReadAllObservations();
        foreach (var obs in repoQueryResult)
        {
            ObservationDto dto = new ObservationDto
            {
                Id = obs.Id,
                AuthorName = obs.Author.Name,
                Text = obs.Text,
                Timestamp = DateTimeToString(obs.TimeStamp),
                TaxonName = obs.Taxon.Name
            };

            observations.Add(dto);
        }
    
        return observations;
    }

    public async Task<List<ObservationDto>> GetObservationsFromAuthor(int authorId, int page = 1)
    {
        int validPage = Math.Max(page, 1);
        List<ObservationDto> observations = new List<ObservationDto>();
        var repoQueryResult = await _PostRepository.ReadObservations(authorId);
        foreach (var obs in repoQueryResult)
        {
            ObservationDto dto = new ObservationDto
            {
                Id = obs.Id,
                AuthorName = obs.Author.Name,
                Text = obs.Text,
                Timestamp = DateTimeToString(obs.TimeStamp),
                TaxonName = obs.Taxon.Name
            };

            observations.Add(dto);
        }

        return observations;
    }
    public async Task<ObservationDto> GetObservationFromId(int observationId, int page = 1)
    {
        var obs = await _PostRepository.ReadSingleObservation(observationId);
        if (obs is null)
            return null; //temp solution
        ObservationDto dto = new ObservationDto
        {
            Id = obs.Id,
            AuthorName = obs.Author.Name,
            Text = obs.Text,
            Timestamp = DateTimeToString(obs.TimeStamp),
            TaxonName = obs.Taxon.Name
        };
        return dto;
    }
    public async Task<List<CommentDto>> GetComments(int observationId)
    {
        List<CommentDto> comments = new List<CommentDto>();

        var repoQueryResult =
            await _PostRepository.ReadComments(observationId);

        foreach (var comment in repoQueryResult)
        {
            CommentDto dto = new CommentDto
            {
                AuthorName = comment.Author.Name,
                Text = comment.Text,
                Timestamp = DateTimeToString(comment.TimeStamp)
            };

            comments.Add(dto);
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
    private static string DateTimeToString(DateTime timestamp)
    {
        return timestamp.ToString("MM/dd/yy H:mm:ss");
    }
}
