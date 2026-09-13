using SimpleDB;

namespace Bison.CLI.Tests;

public class UnitTest1 : IDisposable
{
    private string obsFilePath;
    private string comFilePath;
    private CSVDatabase<ObservationRec> observedb;
    private CSVDatabase<CommentRec> commentdb;

    public UnitTest1()
    {
        obsFilePath = Path.Combine(AppContext.BaseDirectory,"../../../data/obsDB.csv");
        comFilePath = Path.Combine(AppContext.BaseDirectory,"../../../data/comDB.csv");
        observedb = new CSVDatabase<ObservationRec>("obsDB");
        commentdb = new CSVDatabase<CommentRec>("comDB");
    }
    
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