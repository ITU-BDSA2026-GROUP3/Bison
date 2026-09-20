using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Bison.CLI;




namespace Bison.CLI.Tests;

public class ProposalTests : IAsyncLifetime
{
    private const string BaseUrl = "http://localhost:5000";

    private const string ValidTaxonId = "MSTSNM:Arter:3e4e67e4-f785-ea11-aa77-501ac539d1ea";

    private const string InvalidTaxonId = "This-Taxon-ID-does-not-exit";

    private HttpClient client = null!;
    private Process? serverProcess;

    public async Task InitializeAsync()
    {
        client = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl)
        };

        string solutionDirectory = FindSolutionDirectory();

        string projectPath = Path.Combine(solutionDirectory,"src","Bison.CSVDBService","Bison.CSVDBService.csproj");

        serverProcess = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{projectPath}\"",
            WorkingDirectory = solutionDirectory,
            UseShellExecute = false,
            CreateNoWindow = true
        });

        await WaitForServerAsync();
        
    }


    public async Task DisposeAsync()
    {
        client.Dispose();

        if(serverProcess is not null && !serverProcess.HasExited)
        {
            serverProcess.Kill(entireProcessTree:true);
            await serverProcess.WaitForExitAsync();
        }

        serverProcess?.Dispose();
    }

    [Fact]
    public async Task ValidProposalIsStored()
    {
        long observationId = CreateTestId();

        await CreateObservationAsync(observationId);

        ProposalRec proposal = new(observationId, ValidTaxonId);

        HttpResponseMessage response = await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(client,"proposal", proposal);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

List<ProposalRec> proposals = await GetProposalsAsync();
        Assert.Contains(
            proposals, p => p.obsID == observationId && p.taxonID == ValidTaxonId);

}

[Fact]
public async Task InvalidTaxonIsNotStored()
{
    long observationId = CreateTestId();

    await CreateObservationAsync(observationId);

    ProposalRec proposal = new(observationId, InvalidTaxonId);

    HttpResponseMessage response = await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(client,"proposal", proposal);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    List<ProposalRec> proposals = await GetProposalsAsync();

    Assert.DoesNotContain(proposals, p => p.obsID == observationId && p.taxonID == InvalidTaxonId);
}

[Fact]
public async Task invalidObservationIsNotStored()
{
    long observationId = CreateTestId();

    ProposalRec proposal = new (observationId, ValidTaxonId);

    HttpResponseMessage respone = await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(client,"proposal", proposal);

    Assert.Equal(HttpStatusCode.OK, respone.StatusCode);

    List<ProposalRec> proposals = await GetProposalsAsync();

    Assert.DoesNotContain(proposals, p => p.obsID == observationId && p.taxonID == ValidTaxonId);

}

[Fact]
public async Task GetProposalsReturnsStoredProposal()
{
    long observationId = CreateTestId();

    await CreateObservationAsync(observationId);

    ProposalRec proposal = new(observationId,ValidTaxonId);

    await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(client,"proposal", proposal);

    List<ProposalRec> proposals = await GetProposalsAsync();

    Assert.Contains(proposals, p => p.obsID == observationId && p.taxonID == ValidTaxonId);
}

public async Task CreateObservationAsync(long observationId)
    {
        ObservationRec observation = new(observationId,"ProposalTest","Test observation", "Test location", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        HttpResponseMessage response = await  System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(client,"observation", observation);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    
    }

    private async Task<List<ProposalRec>> GetProposalsAsync()
    {
        List<ProposalRec>? proposals = await client.GetFromJsonAsync<List<ProposalRec>>("proposals");

        return proposals ?? new List<ProposalRec>();
    }

    private async Task WaitForServerAsync()
    {
        DateTime timeout = DateTime.UtcNow.AddSeconds(30);

        while (DateTime.UtcNow < timeout)
        {
            try
        
        {
            HttpResponseMessage reponse = await client.GetAsync("proposals");

            if(reponse.IsSuccessStatusCode)
                {
                    return;
                }
        }
            catch (HttpRequestException)
            {
                // The server is still stating
            }

            await Task.Delay(500);

        }

        throw new Exception("Bison.CSVDService did not start within 30 seconds.");
    }

    private static long CreateTestId()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
    }

    private static string FindSolutionDirectory()
    {
        DirectoryInfo? directory= new(AppContext.BaseDirectory);

        while(directory is not null)
        {
            if(directory.GetFiles("*.sln").Length > 0)
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }
        
        throw new DirectoryNotFoundException(
        "Could not find the solution directory.");
        
    }

}