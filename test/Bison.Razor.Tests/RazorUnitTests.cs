using Microsoft.Data.Sqlite;

namespace Bison.Razor.Tests;

[Collection("Sequential Tests")]
public class RazorUnitTests : IDisposable
{
    string observationDatabase;
    string observationTable;
    string commentDatabase;
    string commentTable;
    string proposalDatabase;
    string proposalTable;

    IObservationService service;
    
    public RazorUnitTests()
    {
        DatabaseHandler.InitializeDatabases();
        observationDatabase = DatabaseHandler.observationDatabase;
        observationTable = DatabaseHandler.observationTable;
        commentDatabase = DatabaseHandler.commentDatabase;
        commentTable = DatabaseHandler.commentTable;
        proposalDatabase = DatabaseHandler.proposalDatabase;
        proposalTable = DatabaseHandler.proposalTable;

        service = new ObservationService();
    }

    [Fact]
    public void GetObservationsFromDatabase()
    {
        //Arrange
        var obs1 = new ObservationViewModel(0, "Hjalte", "Email og gullerod", "2G05", "123");
        DatabaseHandler.AddObservation(obs1);

        //Act
        var records = service.GetObservations();

        //Assert
        Assert.NotEmpty(records);
        Assert.Single(records);

        var record = records[0];

        CompareObservations(obs1, record);
    }

    [Fact]
    public void GetObservationsFromEmptyDatabase()
    {
        //Arrange

        //Act
        var records = service.GetObservations();

        //Assert
        Assert.Empty(records);
    }

    [Fact]
    public void GetObservationsFromAuthor()
    {
        //Arrange
        var obs1 = new ObservationViewModel(0, "Hjalte", "Email og gullerod", "2G05", "123");
        var obs2 = new ObservationViewModel(1, "Emil", "Kop med iste", "Bord", "1234");
        var obs3 = new ObservationViewModel(2, "Hjalte", "Bruglig observation", "Brulig Lokation", "1237");
        DatabaseHandler.AddObservation(obs1);
        DatabaseHandler.AddObservation(obs2);
        DatabaseHandler.AddObservation(obs3);

        //Act
        var records = service.GetObservationsFromAuthor("Hjalte");


        //Assert
        Assert.NotEmpty(records);
        Assert.Equal(2, records.Count());

        var rec1 = records[0];
        var rec2 = records[1];

        CompareObservations(obs3, rec1);
        CompareObservations(obs1, rec2);
    }

    [Fact]
    public void GetObservationsFromNonExistingAuthor()
    {
        //Arrange
        var obs1 = new ObservationViewModel(0, "Hjalte", "Email og gullerod", "2G05", "123");
        DatabaseHandler.AddObservation(obs1);

        //Act
        var records = service.GetObservationsFromAuthor("Very Real User And Not Bot About To Hack The Database :)");

        //Assert
        Assert.Empty(records);
    }

    [Fact]
    public void GetCommentsOnObservation()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void GetCommentsOnNonExistingObservation()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void GetProposalsOnObservation()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void GetProposalsOnNonExistingObservation()
    {
        //Arrange


        //Act


        //Assert
        
    }

    [Fact]
    public void UnixTimeStampToDateTimeStringReturnsCorrectTime()
    {
        
    }

    private void CompareObservations(ObservationViewModel obs1, ObservationViewModel obs2)
    {
        Assert.Equal(obs1.obsID, obs2.obsID);
        Assert.Equal(obs1.Author, obs2.Author);
        Assert.Equal(obs1.Message, obs2.Message);
        Assert.Equal(obs1.Location, obs2.Location);
        Assert.Equal(ObservationService.UnixTimeStampToDateTimeString(double.Parse(obs1.Timestamp)), obs2.Timestamp);
    }

    public void Dispose()
    {
        DatabaseHandler.ResetDatabases();
    }
}