public record ObservationViewModel(long obsID, string Author, string Message, string Location ,string Timestamp);
public record CommentViewModel(long commentID, long obsID,string Author, string Comment);
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
            new CommentViewModel(0, 1,  "Peter","OMG where?"),
            new CommentViewModel(1, 0, "Paul", "Nice observation bro!"),
        };

    public List<ObservationViewModel> GetObservations()
    {
        return _obs;
    }

    public List<ObservationViewModel> GetObservationsFromAuthor(string author)
    {
        // filter by the provided author name
        return _obs.Where(x => x.Author == author).ToList();
    }
    public List<CommentViewModel> GetComments(long id)
    {
        // filter by the provided author name
        return _comments.Where(x => x.obsID == id).ToList();
    }

    private static string UnixTimeStampToDateTimeString(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime.ToString("MM/dd/yy H:mm:ss");
    }

}
