using Bison.Razor.domain_model;
using Bison.Razor.DTOs;
using Bison.Razor.Repositories;
using System.Data;
using static System.Net.Mime.MediaTypeNames;

public record ObservationViewModel(long obsID, string Author, string Message, string Location ,string Timestamp);
public record CommentViewModel(long obsID, string Comment);
public interface IObservationService
{
    public Task<List<ObservationDto>> GetObservations(int page = 1);
    public Task<List<ObservationDto>> GetObservationsFromAuthor(string authorId, int page = 1);

    public Task<ObservationDto> GetObservationFromId(int observationId, int page = 1);

    public Task<List<ObservationDto>> GetObservationsFromTaxon(int taxonId, int page = 1);

    public Task<List<CommentDto>> GetComments(int observationId);

    public Task<List<ProposalDto>> GetProposals(int observationId);
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
                AuthorName = obs.Author.UserName,
                Text = obs.Text,
                Timestamp = DateTimeToString(obs.TimeStamp),
                TaxonName = obs.Taxon.Name
            };

            observations.Add(dto);
        }
    
        return observations;
    }

    public async Task<List<ObservationDto>> GetObservationsFromAuthor(string authorId, int page = 1)
    {
        int validPage = Math.Max(page, 1);
        List<ObservationDto> observations = new List<ObservationDto>();
        var repoQueryResult = await _PostRepository.ReadObservations(authorId);
        foreach (var obs in repoQueryResult)
        {
            ObservationDto dto = new ObservationDto
            {
                Id = obs.Id,
                AuthorName = obs.Author.UserName,
                Text = obs.Text,
                Timestamp = DateTimeToString(obs.TimeStamp),
                TaxonName = obs.Taxon.Name
            };

            observations.Add(dto);
        }

        return observations;
    }

    public async Task<List<ObservationDto>> GetObservationsFromTaxon(int taxonId, int page = 1)
    {
        int validPage = Math.Max(page, 1);
        Taxon taxon = await _TaxonRepository.getTaxon(taxonId);
        var observations = await _PostRepository.ReadAllObservations();
        Observation[] observationsArray = observations.ToArray();
        var filt = filterChecked.__default.FilterByChecked(taxon, Dafny.Sequence<Observation>.FromArray(observationsArray));
        List<ObservationDto> filteredObservations = new List<ObservationDto>();
        foreach (var obs in filt)
        {
            ObservationDto dto = new ObservationDto
            {
                Id = obs.Id,
                AuthorName = obs.Author.UserName,
                Text = obs.Text,
                Timestamp = DateTimeToString(obs.TimeStamp),
                TaxonName = obs.Taxon.Name
            };

            filteredObservations.Add(dto);
        }
        return filteredObservations;
    }

    public async Task<ObservationDto> GetObservationFromId(int observationId, int page = 1)
    {
        var obs = await _PostRepository.ReadSingleObservation(observationId);
        if (obs is null)
            return null; //temp solution
        ObservationDto dto = new ObservationDto
        {
            Id = obs.Id,
            AuthorName = obs.Author.UserName,
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
                AuthorName = comment.Author.UserName,
                Text = comment.Text,
                Timestamp = DateTimeToString(comment.TimeStamp)
            };

            comments.Add(dto);
        }

        return comments;
    }

    public async Task<List<ProposalDto>> GetProposals(int observationId)
    {
        List<ProposalDto> proposals = new List<ProposalDto>();

        var repoQueryResult =
            await _PostRepository.ReadProposals(observationId);

        foreach (var proposal in repoQueryResult)
        {
            ProposalDto dto = new ProposalDto
            {
                AuthorName = proposal.Author.UserName,
                Text = proposal.Text,
                Timestamp = DateTimeToString(proposal.TimeStamp),
                TaxonName = proposal.Taxon.Name
            };

            proposals.Add(dto);
        }

        return proposals;
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
