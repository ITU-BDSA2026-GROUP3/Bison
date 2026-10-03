using System.Data;
using static System.Net.Mime.MediaTypeNames;

public record ObservationViewModel(long obsID, string Author, string Message, string Location ,string Timestamp);
public record CommentViewModel(long obsID, string Comment);

public record ProposalViewModel(long obsID, string comment);


public interface IObservationService
{
    public List<ObservationViewModel> GetObservations(int page = 1);
    public List<ObservationViewModel> GetObservationsFromAuthor(string author, int page = 1);

    public List<CommentViewModel> GetComments(long id);

    public ObservationViewModel? GetObservation(long id);

    public List<ProposalViewModel> GetProposals(long id);
}

public class ObservationService : IObservationService
{
    private const int PageSize = 32;
    // These would normally be loaded from a database for example

    private readonly DBFacade _dbFacade;

    public ObservationService(DBFacade dbFacade)
    {
        _dbFacade = dbFacade;
    }
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

    public List<ObservationViewModel> GetObservations(int page = 1)
    {
        int validPage = Math.Max(page, 1);
        List<ObservationViewModel> observations = new List<ObservationViewModel>();
        foreach (Object[] row in _dbFacade.ReadObservations(validPage, PageSize))
        {
            // Fixed the mapping, it is now, row[0] = observation_id, row[2]=username, row[2]=text, row[3] = pub_date
            // There is a "" because location does not the exist in the schema, the sql schema
            observations.Add(new ObservationViewModel((long)row[0], 
                                                        (string)row[1], 
                                                        (string)row[2], 
                                                        "", 
                                                        UnixTimeStampToDateTimeString((long)row[3])));
        }
    
        return observations;
    }

    public List<ObservationViewModel> GetObservationsFromAuthor(string author, int page = 1)
    {
        int validPage = Math.Max(page, 1);
        List<ObservationViewModel> observations = new List<ObservationViewModel>();

        foreach(object[] row in _dbFacade.ReadObservationsFromAuthor(author, validPage, PageSize))
        {
            //Doing the same here as with the GetObservations()
            observations.Add(new ObservationViewModel((long)row[0], (string)row[1], (string)row[2],"", UnixTimeStampToDateTimeString((long)row[3])));
        }

        return observations; // this is now changed
    }
    public List<CommentViewModel> GetComments(long id)
    {
        List<CommentViewModel> comments = new List<CommentViewModel>();
        foreach (Object[] row in _dbFacade.ReadComments())
        {
            comments.Add(new CommentViewModel((long)row[0], (string)row[1]));
        }
;
        return comments.Where(x => x.obsID == id).ToList();
    }

    private static string UnixTimeStampToDateTimeString(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime.ToString("MM/dd/yy H:mm:ss");
    }

    public ObservationViewModel? GetObservation(long id)
    {
        object[]? row = _dbFacade.ReadObservation(id);

        if (row == null)
        {
            return null;
        }

        return new ObservationViewModel(
            (long)row[0],
            (string)row[1],
            (string)row[2],
            "",
            UnixTimeStampToDateTimeString((long)row[3]));
        
    }

    public List<ProposalViewModel> GetProposals(long id)
    {
        List<ProposalViewModel> proposals = new List<ProposalViewModel>();

        foreach (Object[] row in _dbFacade.ReadProposals())
        {
            if((long)row[0] == id)
            {
                proposals.Add(
                    new ProposalViewModel(
                        (long)row[0],
                        (string)row[1]
                    )
                );
            }
            
        }
        
        return proposals;
    }

}
