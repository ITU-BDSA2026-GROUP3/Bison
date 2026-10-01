namespace Bison.Razor.Tests;

[Collection("Sequential Tests")]
public class RazorEndToEndTests : IDisposable, IClassFixture<RazorServiceFixture>
{
    HttpClient Client;
    IObservationService service;
    public RazorEndToEndTests(RazorServiceFixture fixture)
    {
        Client = fixture.Client;

        DatabaseHandler.InitializeDatabases();
        service = new ObservationService();
    }

    [Fact]
    public async Task ObservationsEndToEnd()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public async Task ObservationDetailsEndToEnd()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public async Task ObservationsFromAuthorEndToEnd()
    {
        //Arrange


        //Act


        //Assert
        
    }

    public void Dispose()
    {
        DatabaseHandler.ResetDatabases();
    }
}