using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Bison.CLI;
using Microsoft.AspNetCore.Builder;




namespace Bison.CLI.Tests;
[Collection("Sequential Tests")]
public class ProposalTests : IClassFixture<WebAppFixture>
{
    private const string BaseUrl = "http://localhost:5000";

    private const string ValidTaxonId = "MSTSNM:Arter:3e4e67e4-f785-ea11-aa77-501ac539d1ea";

    private const string InvalidTaxonId = "This-Taxon-ID-does-not-exit";

    private HttpClient client = null!;
    private WebApplication app;

    public ProposalTests(WebAppFixture fixture)
    {
        client = fixture.Client;
        app = fixture.App;
    }

    [Fact]
    public async Task ValidProposalIsStored()
    {
        long observationId = CreateTestId();

        await CreateObservationAsync(observationId);

        ProposalRec proposal = new(observationId, ValidTaxonId);

        HttpResponseMessage response = await client.PostAsJsonAsync("proposal", proposal);

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

    HttpResponseMessage response = await client.PostAsJsonAsync("proposal", proposal);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    List<ProposalRec> proposals = await GetProposalsAsync();

    Assert.DoesNotContain(proposals, p => p.obsID == observationId && p.taxonID == InvalidTaxonId);
}

[Fact]
public async Task invalidObservationIsNotStored()
{
    long observationId = CreateTestId();

    ProposalRec proposal = new (observationId, ValidTaxonId);

    HttpResponseMessage respone = await client.PostAsJsonAsync("proposal", proposal);

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

        await client.PostAsJsonAsync("proposal", proposal);

        List<ProposalRec> proposals = await GetProposalsAsync();

        Assert.Contains(proposals, p => p.obsID == observationId && p.taxonID == ValidTaxonId);
    }

    
    private async Task CreateObservationAsync(long observationId)
    {
        ObservationRec observation = new(observationId,"ProposalTest","Test observation", "Test location", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        HttpResponseMessage response = await  client.PostAsJsonAsync("observation", observation);
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