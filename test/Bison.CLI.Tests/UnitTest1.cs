using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SimpleDB;

namespace Bison.CLI.Tests;

public class UnitTest1 : IDisposable
{
    private string obsFilePath;
    private string comFilePath;

    private string baseURL;
    private HttpClient client;
    private WebApplication app;

    private string name;
    private string location;

    //Constructor
    public UnitTest1()
    {
        obsFilePath = Path.Combine(AppContext.BaseDirectory,"../../../data/obsDB.csv");
        comFilePath = Path.Combine(AppContext.BaseDirectory,"../../../data/comDB.csv");

        name = Environment.UserName;
        location = "Test Location";

        baseURL = "http://localhost:5000";
        client = new()
        {
            BaseAddress = new Uri(baseURL)
        };

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton<IDatabaseRepository,CSVDatabase>();

        var pr = new Bison.CSVDBService.Program();
        app = pr.getApp(builder);

        app.StartAsync().GetAwaiter().GetResult();
    }

    //Unit Tests Start ------------------------------------------------------------------
    [Fact]
    public async Task GetIDSuccesorReturnsNextID()
    {
        var rootCommands = Program.getRootCommands(0, false);
        rootCommands.Parse(new[] {"observe", "test1", location}).Invoke();
        rootCommands.Parse(new[] {"observe", "test2", location}).Invoke();
        rootCommands.Parse(new[] {"observe", "test3", location}).Invoke();

        var ID = await Program.GetIDSuccesor();

        Assert.Equal(3, ID);
    }

    [Fact]
    public async Task GetIDSuccesorEmptyDatabaseReturnsZero()
    {
        var ID = await Program.GetIDSuccesor();

        Assert.Equal(0, ID);
    }

    [Fact]
    public async Task CommentOnExistingDiscussion()
    {
        var rootCommands = Program.getRootCommands(0, false);

        rootCommands.Parse(new[] {"observe", "hello testing observe", location}).Invoke();
        rootCommands.Parse(new[] {"comment", "hello testing comment", "0"}).Invoke();

        var cheeps = await client.GetFromJsonAsync<IEnumerable<CommentRec>>($"comments?id=0");

        var comment = cheeps.LastOrDefault();

        Assert.NotNull(comment);
    }

    [Fact]
    public async Task CantCommentOnNonExistingObservations()
    {
        var rootCommands = Program.getRootCommands(0, false);
        
        rootCommands.Parse(new[] {"comment", "I'm EVIL and commenting on an empty observation >:)", "666"}).Invoke();

        var cheeps = await client.GetFromJsonAsync<IEnumerable<CommentRec>>($"comments?id=0");

        var comment = cheeps.LastOrDefault();

        Assert.Null(comment);
    }

    [Fact]
    public async Task MultipleCommentsOnOneObservation()
    {
        var rootCommands = Program.getRootCommands(0, false);

        rootCommands.Parse(new[] {"observe", "i observe", location}).Invoke();
        rootCommands.Parse(new[] {"comment", "i comment", "0"}).Invoke();
        rootCommands.Parse(new[] {"comment", "me 2", "0"}).Invoke();
        rootCommands.Parse(new[] {"comment", "me 3", "0"}).Invoke();

        var cheeps = await client.GetFromJsonAsync<IEnumerable<CommentRec>>($"comments?id=0");

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
    public async Task ObserveEndToEnd()
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        var rootCommands = Program.getRootCommands(0, false);

        rootCommands.Parse(new[] {"observe", "Penguin", location}).Invoke();

        var records = await client.GetFromJsonAsync<IEnumerable<ObservationRec>>($"cobservations");

        Assert.Single(records);

        var storedRecord = records.LastOrDefault();

        Assert.NotNull(storedRecord);
        Assert.Equal(0, storedRecord.obsID);
        Assert.Equal(name, storedRecord.Author);
        Assert.Equal("Penguin", storedRecord.Observation);
        Assert.Equal(location, storedRecord.Location);
        Assert.True(storedRecord.Timestamp - currentTime < 3); //Check that the observation occured within 3 seconds.
    }

    [Fact]
    public async Task CommentEndToEnd()
    {
        var rootCommands = Program.getRootCommands(0, false);

        rootCommands.Parse(new[] {"observe", "stuff i found", location}).Invoke();

        rootCommands.Parse(new[] {"comment", "Cool stuff", "0"}).Invoke();

        var records = await client.GetFromJsonAsync<IEnumerable<CommentRec>>($"comments?id=0");

        Assert.Single(records);

        var storedRecord = records.LastOrDefault();

        Assert.NotNull(storedRecord);
        Assert.Equal(0, storedRecord.obsID);
        Assert.Equal("Cool stuff", storedRecord.Comment);
    }

    [Fact]
    public async Task ReadEmptyEndToEnd()
    {
        var output = CaptureOutput(() =>
        {
            var rootCommands = Program.getRootCommands(0, false);

            rootCommands.Parse(new[] {"read"}).Invoke();
        });

        Assert.Contains("No prior observations have been made.", output);
        Assert.Contains("Please create an observation before reading.", output);
    }

    [Fact]
    public async Task ReadObservationEndToEnd()
    {
        var obs1 = "i saw something";
        var obs2 = "it was cool";
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var rootCommands = Program.getRootCommands(0, false);

        rootCommands.Parse(new[] {"observe", obs1, location}).Invoke();
        rootCommands.Parse(new[] {"observe", obs2, location}).Invoke();

        var output = CaptureOutput(() =>
        {
            rootCommands.Parse(new[] {"read"}).Invoke();
        });

        var records = await client.GetFromJsonAsync<IEnumerable<CommentRec>>($"observe");

        DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(currentTime).ToLocalTime();

        var dateTime = date.ToString("MM/dd/yy HH:mm:ss");

        Assert.Contains($"{name} @ {dateTime}: {obs1}", output);
        Assert.Contains($"{name} @ {dateTime}: {obs2}", output);
    }

    [Fact]
    public async Task DiscussionEndToEnd()
    {
        var rootCommands = Program.getRootCommands(0, false);
        
        rootCommands.Parse(new[] {"observe", "stuff i found", location}).Invoke();

        rootCommands.Parse(new[] {"comment", "Cool stuff", "0"}).Invoke();

        var output = CaptureOutput(() =>
        {
            var rootCommands = Program.getRootCommands(0, false);

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
        
        if(app != null)
        {
            app.StopAsync().GetAwaiter().GetResult();
            app.DisposeAsync().GetAwaiter().GetResult();
        }

        client?.Dispose();
    }
}