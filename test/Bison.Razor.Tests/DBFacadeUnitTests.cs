using Microsoft.Data.Sqlite;

namespace Bison.Razor.Tests;

[Collection("Sequential Tests")]
public class DBFacadeUnitTests : IDisposable
{
    private static readonly string DatabasePath = Path.GetFullPath(
        Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../data"));
    
    private DBFacade db = new DBFacade();

    public DBFacadeUnitTests()
    {
        DatabaseHandler.InitializeDatabases(db);
    }

    [Fact]
    public void ReadFromEmptyDatabase()
    {
        //Arrange


        //Act
        var records = db.ReadDatabase("observation");

        //Assert
        Assert.Empty(records);
    }
    
    [Fact]
    public void ReadFromDatabase()
    {
        //Arrange
        DatabaseHandler.AddUser(0, "Emil", "emil@mail", "hash");
        DatabaseHandler.AddObservation(new ObservationViewModel(0, "Emil", "Test", "Test Location", "1234321"), 0);

        //Act
        var records = db.ReadDatabase("observation");

        //Assert
        Assert.NotEmpty(records);
        Assert.Single(records);

        var record = records[0];

        Assert.Equal(0L, record[0]); //obsID
        Assert.Equal(0L, record[1]); //Author ID
        Assert.Equal("Test", record[2]); //Observation
        Assert.Equal(1234321L, record[3]); //Timestamp
    }

    public void Dispose()
    {
        DatabaseHandler.ResetDatabases();
    }
}