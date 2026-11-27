using Bison.Core.domain_model;
using Bison.infrastructure.DTOs;
using Bison.infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.Data;
using System.Security.Cryptography.X509Certificates;
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
    public Task<AuthorDto?> GetAuthor(string authorId);

    public Task ForgetUser(string authorId, bool keepObservations);

    public Task<List<int>> CreateObservation(string text, string authorID, int taxonID);
    public Task<List<int>> CreateObservation(string text, string authorID, int taxonID, byte[] imgbytes, string licence);
    public Task<List<int>> CreateComment(string text, string authorID, int observationID);
    public Task<List<int>> CreateProposal(string text, string authorID, int observationID, int taxonID);
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

    public async Task ForgetUser(string authorId, bool keepObservations)
    {
        await _PostRepository.HandleForgottenUserObservations(authorId, keepObservations);
        await _AuthorRepository.AnonymizeAuthor(authorId);
    }


    public async Task<List<int>> CreateComment(string text, string authorID, int observationID)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("The comment cannot be empty.");
        }

        AuthorDto author = await _AuthorRepository.getAuthor(authorID);

        if (author == null)
        {
            throw new InvalidOperationException("Author not found.");
        }

        var observation = await _PostRepository.ReadSingleObservation(observationID);

        if (observation == null)
        {
            throw new InvalidOperationException("Observation not found.");
        }

        var comment = new CommentDto
        {
            obsId = observationID,
            Text = text.Trim(),
            AuthorId =authorID,
            AuthorName = author.Name
            
        };

        return await _PostRepository.CreateComments(
            new List<CommentDto> { comment });
    }

    public async Task<List<int>> CreateProposal(string text, string authorID, int observationID,int taxonID)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("The proposal cannot be empty.");
        }

        var author = await _AuthorRepository.getAuthor(authorID);

        if (author == null)
        {
            throw new InvalidOperationException("Author not found.");
        }

        var observation = await _PostRepository.ReadSingleObservation(observationID);

        if (observation == null)
        {
            throw new InvalidOperationException("Observation not found.");
        }

        var taxon = await _TaxonRepository.getTaxon(taxonID);

        if (taxon == null)
        {
            throw new InvalidOperationException("Taxon not found.");
        }

        var proposal = new ProposalDto
        {
            obsId = observationID,
            Text = text.Trim(),
            AuthorName = author.Name,
            AuthorId = authorID,
            TaxonName = taxon.Name
        };

        return await _PostRepository.CreateProposals(
            new List<ProposalDto> { proposal });
    }
    
    public async Task<List<int>> CreateObservation(string text, string authorID, int taxonID)
    {
        if (string.IsNullOrWhiteSpace(text)){
            throw new ArgumentException("The Observation cannot be empty.");
        }

        //This retrieves the author of obs
        AuthorDto author= await _AuthorRepository.getAuthor(authorID);

        if(author == null)
        {
            throw new InvalidOperationException("Author not found.");
        }

        //Retrieve the taxon
        var taxon = await _TaxonRepository.getTaxon(taxonID);

        if(taxon == null)
        {
            throw new InvalidOperationException("Taxon not found");
        }


        //Creating the observation
        var observation = new ObservationDto
        {
            Text = text.Trim(),
            AuthorName = author.Name,
            TaxonName = taxon.Name,
        };

        return await _PostRepository.CreateObservations(new List<ObservationDto> {observation});
    }

    //overload
    public async Task<List<int>> CreateObservation(string text, string authorID, int taxonID, byte[] imgbytes, string licence)
    {
        if (string.IsNullOrWhiteSpace(text)){
            throw new ArgumentException("The Observation cannot be empty.");
        }

        //This retrieves the author of obs
        AuthorDto author= await _AuthorRepository.getAuthor(authorID);

        if(author == null)
        {
            throw new InvalidOperationException("Author not found.");
        }

        //Retrieve the taxon
        Taxon? taxon = null;
        if (taxonID != 0) {
            taxon = await _TaxonRepository.getTaxon(taxonID);
            if(taxon == null)
            {
                throw new InvalidOperationException("Taxon not found");
            }
        }

        
        ObservationDto observation;
        if(imgbytes == null || imgbytes.Length == 0)
        //Creating the observation
        observation = new ObservationDto
        {
            Text = text.Trim(),
            AuthorName = author.Name,
            TaxonName = taxon.Name,
        };
        else if(licence != null) 
        observation = new ObservationDto
        {
            Text = text.Trim(),
            AuthorName = author.Name,
            TaxonName = taxon.Name,
            ImgBytes = imgbytes,
            Licence = licence    
        };
        else observation = new ObservationDto
        {
            Text = text.Trim(),
            AuthorName = author.Name,
            TaxonName = taxon.Name,
            ImgBytes = imgbytes
        };

        return await _PostRepository.CreateObservations(new List<ObservationDto> {observation});
    }

    private const int PageSize = 32;
    // These would normally be loaded from a database for example
   

    public async Task<List<ObservationDto>> GetObservations(int page = 1)
    {

        int validPage = Math.Max(page, 1);
        List<ObservationDto> observations = new List<ObservationDto>();
        var repoQueryResult = await _PostRepository.ReadAllObservations();
        
        return repoQueryResult;
    }

    public async Task<List<ObservationDto>> GetObservationsFromAuthor(string authorId, int page = 1)
    {
        int validPage = Math.Max(page, 1);
        List<ObservationDto> observations = new List<ObservationDto>();
        var repoQueryResult = await _PostRepository.ReadObservations(authorId);

        return repoQueryResult;
    }

    public async Task<List<ObservationDto>> GetObservationsFromTaxon(int taxonId, int page = 1)
    {
        int validPage = Math.Max(page, 1);
        var taxon = await _TaxonRepository.getTaxon(taxonId);
        if(taxon == null) return null;
        var observations = await _PostRepository.ReadAllObservations();
        Observation[] observationsArray = observations.ToArray();
        var filt = filterChecked.__default.FilterByChecked(taxon, Dafny.Sequence<Observation>.FromArray(observationsArray));
        List<Observation> filteredObservations = new List<Observation>();
        foreach (var obs in filt)
        {
            ObservationDto dto = new ObservationDto
            {
                Id = obs.Id,
                AuthorName = obs.Author?.UserName ?? "deleted user",
                Text = obs.Text,
                Timestamp = DateTimeToString(obs.TimeStamp),
                ImgBytes = obs.ImgBytes, //imgbytes can be null in that case handle it the same as if there is no bytes(very unlikely to ever happen) 
                TaxonName = obs.Taxon.Name,
                Licence = obs.Licence
            };

            filteredObservations.Add(dto);
        }
        return filteredObservations;
    }

    public async Task<ObservationDto> GetObservationFromId(int observationId, int page = 1)
    {
        var obs = await _PostRepository.ReadSingleObservation(observationId);
        if (obs == null)
        {
            throw new InvalidOperationException("Observation not found.");
        }
        return obs;
    }
    public async Task<List<CommentDto>> GetComments(int observationId)
    {
        var repoQueryResult =
            await _PostRepository.ReadComments(observationId);


        return repoQueryResult;
    }

    public async Task<List<ProposalDto>> GetProposals(int observationId)
    {

        var repoQueryResult =
            await _PostRepository.ReadProposals(observationId);
        return repoQueryResult;
    }

    public async Task<AuthorDto?> GetAuthor(string authorId)
    {

        AuthorDto repoQueryResult = await _AuthorRepository.getAuthor(authorId);
        if (repoQueryResult == null)
        {
            return null;
        }
        AuthorDto dto = new AuthorDto
        {
            Id = repoQueryResult.Id,
            Name = repoQueryResult.Name,
            Email = repoQueryResult.Email
        };
        return dto;

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
