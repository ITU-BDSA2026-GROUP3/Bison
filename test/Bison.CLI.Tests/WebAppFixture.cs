using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SimpleDB;

namespace Bison.CLI.Tests;

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