using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SimpleDB;

namespace Bison.CLI.Tests;

[Collection("Sequential Tests")]
public class UnitTest1 : IDisposable, IClassFixture<WebAppFixture>
{
    private string obsFilePath;
    private string comFilePath;

    private HttpClient client;
    private WebApplication app;

    private string name;
    private string location;

    //Constructor
    public UnitTest1(WebAppFixture fixture)
    {
        obsFilePath = fixture.obsFilePath;
        comFilePath = fixture.comFilePath;

        name = Environment.UserName;
        location = "Test Location";
        
        app = fixture.App;
        client = fixture.Client;

        ResetCSVFiles();
    }

    //Unit Tests Start ------------------------------------------------------------------
    [Fact]
    public async Task GetIDSuccesorReturnsNextID()
    {
        var rootCommands = Program.getRootCommands(0, false);
        await rootCommands.Parse(new[] {"observe", "test1", location}).InvokeAsync();
        rootCommands = Program.getRootCommands(0, false);
        await rootCommands.Parse(new[] {"observe", "test2", location}).InvokeAsync();
        rootCommands = Program.getRootCommands(0, false);
        await rootCommands.Parse(new[] {"observe", "test3", location}).InvokeAsync();

        var ID = await Program.GetIDSuccesor();

        var Count = await client.GetFromJsonAsync<IEnumerable<ObservationRec>>("observations");

        Assert.Equal(Count.Count(), ID);
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

        await rootCommands.Parse(new[] {"observe", "hello testing observe", location}).InvokeAsync();
        await rootCommands.Parse(new[] {"comment", "hello testing comment", "0"}).InvokeAsync();

        var cheeps = await client.GetFromJsonAsync<IEnumerable<CommentRec>>($"comments?id=0");

        var comment = cheeps.LastOrDefault();

        Assert.NotNull(comment);
    }

    [Fact]
    public async Task CantCommentOnNonExistingObservations()
    {
        var rootCommands = Program.getRootCommands(0, false);
        
        await rootCommands.Parse(new[] {"comment", "I'm EVIL and commenting on an empty observation >:)", "666"}).InvokeAsync();

        var cheeps = await client.GetFromJsonAsync<IEnumerable<CommentRec>>($"comments?id=0");

        var comment = cheeps.LastOrDefault();

        Assert.Null(comment);
    }

    [Fact]
    public async Task MultipleCommentsOnOneObservation()
    {
        var rootCommands = Program.getRootCommands(0, false);

        await rootCommands.Parse(new[] {"observe", "i observe", location}).InvokeAsync();
        await rootCommands.Parse(new[] {"comment", "i comment", "0"}).InvokeAsync();
        await rootCommands.Parse(new[] {"comment", "me 2", "0"}).InvokeAsync();
        await rootCommands.Parse(new[] {"comment", "me 3", "0"}).InvokeAsync();

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

        await rootCommands.Parse(new[] {"observe", "Penguin", location}).InvokeAsync();

        var records = await client.GetFromJsonAsync<IEnumerable<ObservationRec>>($"observations");

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

        await rootCommands.Parse(new[] {"observe", "stuff i found", location}).InvokeAsync();

        await rootCommands.Parse(new[] {"comment", "Cool stuff", "0"}).InvokeAsync();

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
        var output = CaptureOutput(async () =>
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
        await rootCommands.Parse(new[] {"observe", obs1, location}).InvokeAsync();

        rootCommands = Program.getRootCommands(0, false);
        await rootCommands.Parse(new[] {"observe", obs2, location}).InvokeAsync();

        var output = CaptureOutput(async () =>
        {
            rootCommands.Parse(new[] {"read"}).Invoke();
        });

        var records = await client.GetFromJsonAsync<IEnumerable<ObservationRec>>($"observations");

        DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(currentTime).ToLocalTime();

        var dateTime = date.ToString("MM/dd/yy HH:mm:ss");

        Assert.Contains($"{name} @ {dateTime}: {obs1}", output);
        Assert.Contains($"{name} @ {dateTime}: {obs2}", output);
    }

    [Fact]
    public async Task DiscussionEndToEnd()
    {
        var rootCommands = Program.getRootCommands(0, false);
        
        await rootCommands.Parse(new[] {"observe", "stuff i found", location}).InvokeAsync();

        await rootCommands.Parse(new[] {"comment", "Cool stuff", "0"}).InvokeAsync();

        var output = CaptureOutput(async () =>
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
    public void ResetCSVFiles()
    {
        if (File.Exists(obsFilePath))
        {
            File.WriteAllText(obsFilePath, string.Empty);
        }

        if (File.Exists(comFilePath))
        {
            File.WriteAllText(comFilePath, string.Empty);
        }
    }
    public void Dispose()
    {
        ResetCSVFiles();
    }
}

public class WebAppFixture : IAsyncLifetime
{
    public WebApplication App { get; private set; }
    public HttpClient Client { get; private set; }
    public string BaseURL { get; } = "http://localhost:5000";

    public string obsFilePath;
    public string comFilePath;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton<IDatabaseRepository, CSVDatabase>();

        App = Bison.CSVDBService.Program.getApp(builder, new[] {"test"});

        // Start serveren asynkront én gang
        await App.StartAsync();

        Client = new HttpClient { BaseAddress = new Uri(BaseURL) };

        obsFilePath = Path.Combine(AppContext.BaseDirectory,"../../../../../data/test_observe_cli_db.csv");
        comFilePath = Path.Combine(AppContext.BaseDirectory,"../../../../../data/test_comment_cli_db.csv");
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (App != null)
        {
            await App.StopAsync();
            await App.DisposeAsync();
        }

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