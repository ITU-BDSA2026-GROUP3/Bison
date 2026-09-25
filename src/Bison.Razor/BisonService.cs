using System.Data;
using static System.Net.Mime.MediaTypeNames;

public record ObservationViewModel(long obsID, string Author, string Message, string Location ,string Timestamp);
public record CommentViewModel(long obsID, string Comment);
public interface IObservationService
{
    public List<ObservationViewModel> GetObservations();
    public List<ObservationViewModel> GetObservationsFromAuthor(string author);

    public List<CommentViewModel> GetComments(long id);
}

public class ObservationService : IObservationService
{
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

    public List<ObservationViewModel> GetObservations()
    {
        List<ObservationViewModel> observations = new List<ObservationViewModel>();
        foreach (Object[] row in DBFacade.ReadObservations())
        {
            observations.Add(new ObservationViewModel((long)row[0], (string)row[1], (string)row[2], (string)row[3], UnixTimeStampToDateTimeString((long)row[4])));
        }
    
        return observations;
    }

    public List<ObservationViewModel> GetObservationsFromAuthor(string author)
    {
        // filter by the provided author name
        return GetObservations().Where(x => x.Author == author).ToList(); // this is lazy and needs to be changed >:(
    }
    public List<CommentViewModel> GetComments(long id)
    {
        List<CommentViewModel> comments = new List<CommentViewModel>();
        foreach (Object[] row in DBFacade.ReadObservations())
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

}
