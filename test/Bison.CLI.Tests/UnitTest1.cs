using SimpleDB;

namespace Bison.CLI.Tests;

public class UnitTest1 : IDisposable
{
    private string obsFilePath;
    private string comFilePath;
    private IDatabaseRepository<ObservationRec> observedb;
    private IDatabaseRepository<CommentRec> commentdb;

    //Constructor
    public UnitTest1()
    {
        obsFilePath = Path.Combine(AppContext.BaseDirectory,"../../../data/obsDB.csv");
        comFilePath = Path.Combine(AppContext.BaseDirectory,"../../../data/comDB.csv");
        observedb = new CSVDatabase<ObservationRec>("obsDB");
        commentdb = new CSVDatabase<CommentRec>("comDB");
    }

    //Unit Tests Start ------------------------------------------------------------------
    [Fact]
    public void GetIDSuccesorReturnsNextID()
    {
        observedb.Store(new ObservationRec(0, "Tester", "test1", 2));
        observedb.Store(new ObservationRec(1, "Tester", "test2", 6));
        observedb.Store(new ObservationRec(2, "Tester", "test3", 12));

        var ID = Program.GetIDSuccesor(observedb);

        Assert.Equal(3, ID);
    }

    [Fact]
    public void GetIDSuccesorEmptyDatabaseReturnsZero()
    {
        var ID = Program.GetIDSuccesor(observedb);

        Assert.Equal(0, ID);
    }

    [Fact]
    public void CommentOnExistingDiscussion()
    {
        var rootCommands = Program.getRootCommands(observedb, commentdb, 0);

        rootCommands.Parse(new[] {"observe", "hello testing observe"}).Invoke();
        rootCommands.Parse(new[] {"comment", "hello testing comment", "0"}).Invoke();

        var cheeps = commentdb.Read();

        var comment = cheeps.LastOrDefault();

        Assert.NotNull(comment);
    }

    [Fact]
    public void CantCommentOnNonExistingObservations()
    {
        var rootCommands = Program.getRootCommands(observedb, commentdb, 0);
        
        rootCommands.Parse(new[] {"comment", "I'm EVIL and commenting on an empty observation >:)", "666"}).Invoke();

        var cheeps = commentdb.Read();

        var comment = cheeps.LastOrDefault();

        Assert.Null(comment);
    }

    [Fact]
    public void MultipleCommentsOnOneObservation()
    {
        var rootCommands = Program.getRootCommands(observedb, commentdb, 0);

        rootCommands.Parse(new[] {"observe", "i observe"}).Invoke();
        rootCommands.Parse(new[] {"comment", "i comment", "0"}).Invoke();
        rootCommands.Parse(new[] {"comment", "me 2", "0"}).Invoke();
        rootCommands.Parse(new[] {"comment", "me 3", "0"}).Invoke();

        var cheeps = commentdb.Read();

        var commentCounter = 0;

        foreach(CommentRec cheep in cheeps)
        {
            if(cheep.obsID == 0) commentCounter++;
        }

        Assert.Equal(3, commentCounter);
    }

    //Unit Tests End ------------------------------------------------------------------

    //End To End Tests Start ----------------------------------------------------------

    [Fact]
    public void ObserveEndToEnd()
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        var rootCommands = Program.getRootCommands(observedb, commentdb, 0);

        rootCommands.Parse(new[] {"observe", "Penguin"}).Invoke();

        var records = observedb.Read();

        Assert.Single(records);

        var storedRecord = records.LastOrDefault();

        Assert.NotNull(storedRecord);
        Assert.Equal(0, storedRecord.obsID);
        Assert.Equal(Environment.UserName, storedRecord.Author);
        Assert.Equal("Penguin", storedRecord.Observation);
        Assert.True(storedRecord.Timestamp - currentTime < 3); //Check that the observation occured within 3 seconds.
    }

    [Fact]
    public void CommentEndToEnd()
    {
        observedb.Store(new ObservationRec(0, "me", "stuff i found", 123));

        var rootCommands = Program.getRootCommands(observedb, commentdb, 0);

        rootCommands.Parse(new[] {"comment", "Cool stuff", "0"}).Invoke();

        var records = commentdb.Read();

        Assert.Single(records);

        var storedRecord = records.LastOrDefault();

        Assert.NotNull(storedRecord);
        Assert.Equal(0, storedRecord.obsID);
        Assert.Equal("Cool stuff", storedRecord.Comment);
    }

    [Fact]
    public void ReadEmptyEndToEnd()
    {
        var output = CaptureOutput(() =>
        {
            var rootCommands = Program.getRootCommands(observedb, commentdb, 0);

            rootCommands.Parse(new[] {"read"}).Invoke();
        });

        Assert.Contains("No prior observations have been made.", output);
        Assert.Contains("Please create an observation before reading.", output);
    }

    [Fact]
    public void ReadObservationEndToEnd()
    {
        var obs1 = "i saw something";
        var obs2 = "it was cool";
        var author = Environment.UserName;
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        observedb.Store(new ObservationRec(0, author, obs1, currentTime));
        observedb.Store(new ObservationRec(1, author, obs2, currentTime));

        var output = CaptureOutput(() =>
        {
            var rootCommands = Program.getRootCommands(observedb, commentdb, 0);

            rootCommands.Parse(new[] {"read"}).Invoke();
        });

        DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(currentTime).ToLocalTime();

        var dateTime = date.ToString("MM/dd/yy HH:mm:ss");

        Assert.Contains($"{author} @ {dateTime}: {obs1}", output);
        Assert.Contains($"{author} @ {dateTime}: {obs2}", output);
    }

    [Fact]
    public void DiscussionEndToEnd()
    {
        observedb.Store(new ObservationRec(0, "me", "stuff i found", 123));
        commentdb.Store(new CommentRec(0, "Cool stuff"));

        var output = CaptureOutput(() =>
        {
            var rootCommands = Program.getRootCommands(observedb, commentdb, 0);

            rootCommands.Parse(new[] {"discussion", "0"}).Invoke();
        });

        Assert.Contains("Cool stuff", output);
    }


    //Captures strings printed to the console
    private static string CaptureOutput(Action action)
    {
        using var output = new StringWriter();

        var originalOut = Console.Out;
        Console.SetOut(output);

        action();

        Console.SetOut(originalOut);
        return output.ToString();
    }


    //File Handling
    public void Dispose()
    {
        if (File.Exists(obsFilePath))
        {
            File.Delete(obsFilePath);
        }

        if (File.Exists(comFilePath))
        {
            File.Delete(comFilePath);
        }
    }
}