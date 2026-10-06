namespace Bison.Razor.Tests;

[Collection("Sequential Tests")]
public class RazorEndToEndTests : IDisposable, IClassFixture<RazorServiceFixture>
{
    HttpClient Client;
    public RazorEndToEndTests(RazorServiceFixture fixture)
    {
        Client = fixture.CreateClient();
        DatabaseHandler.InitializeDatabases(fixture.db);
    }

    [Fact]
    public async Task ObservationsEndToEnd()
    {
        //Arrange
        var obs1 = new ObservationViewModel(0, "Emil", "Sej gut", "Lejlighed", "123454321");
        DatabaseHandler.AddUser(0, "Emil", "Emil@mail", "hash");
        DatabaseHandler.AddObservation(obs1, 0);

        //Act
        var response = await Client.GetAsync("/obs");
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Contains("sp-route-author=\"Emil\">", content);
        Assert.Contains("Sej gut", content);
        Assert.Contains(ObservationService.UnixTimeStampToDateTimeString(123454321L), content);
    }

    [Fact]
    public async Task NoObservationsEndToEnd()
    {
        //Arrange

        //Act
        var response = await Client.GetAsync("/obs");
        var content = await response.Content.ReadAsStringAsync();

        Console.WriteLine(content);

        //Assert
        Assert.Contains("<em>There are no observations so far.</em>", content);
    }

    [Fact]
    public async Task ObservationDetailsEndToEnd()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public async Task NoObservationDetialsEndToEnd()
    {
        //Arrange


        //Act


        //Assert
    }

    [Fact]
    public async Task ObservationsFromAuthorEndToEnd()
    {
        //Arrange
        var obs1 = new ObservationViewModel(0, "Emil", "GIGA tyk egern", "Udenfor", "12345");
        var obs2 = new ObservationViewModel(1, "Jonas", "*Downvote Post", "Skummel Lokation", "12356");
        var obs3 = new ObservationViewModel(2, "Emil", "Hvem downvotede mit post :(", "Lejlighed", "12399");
        
        DatabaseHandler.AddUser(0, "Emil", "emil@mail", "hash");
        DatabaseHandler.AddUser(1, "Jonas", "jonas@mail", "hash2");

        DatabaseHandler.AddObservation(obs1, 0);
        DatabaseHandler.AddObservation(obs2, 1);
        DatabaseHandler.AddObservation(obs3, 0);

        //Act
        var response = await Client.GetAsync("/obs/Emil");
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Contains("Emil's Observations", content);

        //First observation
        Assert.Contains("sp-route-author=\"Emil\">", content);
        Assert.Contains("GIGA tyk egern", content);
        Assert.Contains(ObservationService.UnixTimeStampToDateTimeString(12345L), content);

        //Second observation
        Assert.DoesNotContain("sp-route-author=\"Jonas\">", content);
        Assert.DoesNotContain("*Downvote Post", content);
        Assert.DoesNotContain(ObservationService.UnixTimeStampToDateTimeString(12346L), content);

        //Third observation
        Assert.Contains("sp-route-author=\"Emil\">", content);
        Assert.Contains("Hvem downvotede mit post :(", content);
        Assert.Contains(ObservationService.UnixTimeStampToDateTimeString(12399L), content);
    }

    [Fact]
    public async Task ObservationsFromNonExistingAuthorEndToEnd()
    {
        //Arrange
        var obs1 = new ObservationViewModel(0, "Morten", "Hvem fanden er Morten?", "???", "123");
        DatabaseHandler.AddUser(0, "Morten", "mail", "hash");
        DatabaseHandler.AddObservation(obs1, 0);

        //Act
        var response = await Client.GetAsync("/obs/Emil");
        var content = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Contains("Emil's Observations", content);
        Assert.Contains("<em>There are no Observations so far.</em>", content);

        //Martin observation
        Assert.DoesNotContain("sp-route-author=\"Morten\">", content);
        Assert.DoesNotContain("Hvem fanden er Morten?", content);
        Assert.DoesNotContain(ObservationService.UnixTimeStampToDateTimeString(123L), content);
    }

    public void Dispose()
    {
        DatabaseHandler.ResetDatabases();
    }
}