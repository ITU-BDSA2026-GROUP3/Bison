using Microsoft.Data.Sqlite;

namespace Bison.Razor.Tests;

[Collection("Sequential Tests")]
public class RazorUnitTests : IDisposable
{
    IObservationService service;
    DBFacade db;
    
    public RazorUnitTests()
    {
        db = new DBFacade();
        
        DatabaseHandler.InitializeDatabases(db);

        service = new ObservationService(db);
    }

    [Fact]
    public void GetObservationsFromDatabase()
    {
        //Arrange
        var obs1 = new ObservationViewModel(0, "Hjalte", "Email og gullerod", "2G05", "123");
        DatabaseHandler.AddUser(0, "Hjalte", "mail", "hash");
        DatabaseHandler.AddObservation(obs1, 0);

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
        DatabaseHandler.AddUser(0, "Emil", "email", "hash");
        DatabaseHandler.AddUser(1, "Hjalte", "hmail", "hash2");
        DatabaseHandler.AddObservation(obs1, 1);
        DatabaseHandler.AddObservation(obs2, 0);
        DatabaseHandler.AddObservation(obs3, 1);

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
        DatabaseHandler.AddUser(0, "Hjalte", "hmail", "hash");
        DatabaseHandler.AddObservation(obs1, 0);

        //Act
        var records = service.GetObservationsFromAuthor("Very Real User And Not Bot About To Hack The Database :)");

        //Assert
        Assert.Empty(records);
    }

    [Fact]
    public void GetCommentsOnObservation()
    {
        //Arrange
        var obs1 = new ObservationViewModel(0L, "Emil", "Hjalte im not doing that", "her", "123");
        var obs2 = new ObservationViewModel(1L, "THe dEVil", "DONT COMMENT ON THIS OR YOU DIEEE", "kfc", "1232");
        var com = new CommentViewModel(1L, "I COMMENTED HIHIIHIHIHIH");
        DatabaseHandler.AddUser(0, "Emil", "email", "hash");
        DatabaseHandler.AddUser(1, "THe dEVil", "dmail", "hash2");
        DatabaseHandler.AddObservation(obs1, 0);
        DatabaseHandler.AddObservation(obs2, 1);
        DatabaseHandler.AddComment(com);

        //Act
        var comments = service.GetComments(obs2.obsID);

        //Assert
        Assert.NotEmpty(comments);
        Assert.Single(comments);

        var comment = comments[0];
        CompareComments(com, comment);
    }

    
    //NEEDS LOGIC WHEN INSERTING COMMENTS TO WORK. CAN'T CURRENTLY TEST :(
    /*[Fact]
    public void GetCommentsOnNonExistingObservation()
    {
        //Arrange
        var com = new CommentViewModel(0L, "I COMMENTED HIHIIHIHIHIH");
        DatabaseHandler.AddComment(com);

        //Act
        var comments = service.GetComments(0L);

        //Assert
        Assert.Empty(comments);
    }*/

    [Fact]
    public void GetMultipleCommentsOnObservation()
    {
        //Arrange
        var obs = new ObservationViewModel(0L, "THe dEVil", "DONT COMMENT ON THIS OR YOU DIEEE", "kfc", "1232");
        var com1 = new CommentViewModel(0L, "I COMMENTED HIHIIHIHIHIH");
        var com2 = new CommentViewModel(0L, "ME TOO LOLOLOLOLO");
        DatabaseHandler.AddUser(0, "THe dEVil", "dmail", "hash");
        DatabaseHandler.AddObservation(obs, 0);
        DatabaseHandler.AddComment(com1);
        DatabaseHandler.AddComment(com2);

        //Act
        var comments = service.GetComments(0L);

        //Assert
        Assert.NotEmpty(comments);
        Assert.Equal(2, comments.Count());

        var comment1 = comments[0];
        var comment2 = comments[1];

        CompareComments(com1, comment1);
        CompareComments(com2, comment2);
    }

    [Fact]
    public void GetProposalsOnObservation()
    {
        //Arrange
        var obs = new ObservationViewModel(0L, "Emil", "Tror det her er en kanin. Nogen der kan factchecke?", "zoo", "123");
        var prop = new ProposalViewModel(0L, "Abekat");
        
        DatabaseHandler.AddUser(0, "Emil", "email", "hash");
        DatabaseHandler.AddObservation(obs, 0);
        DatabaseHandler.AddProposal(prop);

        //Act
        var proposals = service.GetProposals(0L);

        //Assert
        Assert.NotEmpty(proposals);
        Assert.Single(proposals);

        var proposal = proposals[0];

        CompareProposals(prop, proposal);
    }

    //NEEDS LOGIC WHEN INSERTING PROPOSALS TO WORK. CAN'T CURRENTLY TEST :(
    /*[Fact]
    public void GetProposalsOnNonExistingObservation()
    {
        //Arrange


        //Act


        //Assert
        
    }*/

    [Fact]
    public void UnixTimeStampToDateTimeStringReturnsCorrectTime()
    {
        //Arrange
        string time1 = "01-01-70 0:02:03";
        var unixtime1 = 123L;

        string time2 = "10-01-26 10:29:22";
        var unixtime2 = 1790850562L;

        //Act
        var convertedTime1 = ObservationService.UnixTimeStampToDateTimeString(unixtime1);
        var convertedTime2 = ObservationService.UnixTimeStampToDateTimeString(unixtime2);

        //Assert
        Assert.Equal(time1, convertedTime1);
        Assert.Equal(time2, convertedTime2);
    }

    private void CompareObservations(ObservationViewModel obs1, ObservationViewModel obs2)
    {
        Assert.NotNull(obs1);
        Assert.NotNull(obs2);

        Assert.Equal(obs1.obsID, obs2.obsID);
        Assert.Equal(obs1.Author, obs2.Author);
        Assert.Equal(obs1.Message, obs2.Message);
        Assert.Equal(ObservationService.UnixTimeStampToDateTimeString(double.Parse(obs1.Timestamp)), obs2.Timestamp);
    }

    private void CompareComments(CommentViewModel com1, CommentViewModel com2)
    {
        Assert.NotNull(com1);
        Assert.NotNull(com2);

        Assert.Equal(com1.obsID, com2.obsID);
        Assert.Equal(com1.Comment, com2.Comment);
    }

    private void CompareProposals(ProposalViewModel prop1, ProposalViewModel prop2)
    {
        Assert.NotNull(prop1);
        Assert.NotNull(prop2);

        Assert.Equal(prop1.obsID, prop2.obsID);
        Assert.Equal(prop1.taxonID, prop2.taxonID);
    }

    public void Dispose()
    {
        DatabaseHandler.ResetDatabases();
    }
}