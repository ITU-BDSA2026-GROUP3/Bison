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
        
    }
}