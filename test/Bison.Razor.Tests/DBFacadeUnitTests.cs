using Microsoft.Data.Sqlite;

namespace Bison.Razor.Tests;

[Collection("Sequential Tests")]
public class DBFacadeUnitTests : IDisposable
{
    private static readonly string DatabasePath = Path.GetFullPath(
        Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../data"));
    
    const string observationDatabase = "test_observe_cli.db";
    const string tableName = "test_observe_cli_db";

    public DBFacadeUnitTests()
    {
        DBFacade.InitializeDatabase(
            "test_observe_cli.db",
            """
            CREATE TABLE IF NOT EXISTS test_observe_cli_db (
                obsID INTEGER PRIMARY KEY,
                Author TEXT NOT NULL,
                Observation TEXT NOT NULL,
                Location TEXT NOT NULL,
                Timestamp INTEGER NOT NULL
            );
            """);
    }

    [Fact]
    public void ReadFromEmptyDatabase()
    {
        //Arrange


        //Act
        var records = DBFacade.ReadDatabase(observationDatabase, tableName);

        //Assert
        Assert.Empty(records);
    }
    
    [Fact]
    public void ReadFromDatabase()
    {
        //Arrange
        DatabaseHandler.AddObservation(new ObservationViewModel(0, "Emil", "Test", "Test Location", "1234321"), observationDatabase, tableName);

        //Act
        var records = DBFacade.ReadDatabase(observationDatabase, tableName);

        //Assert
        Assert.NotEmpty(records);
        Assert.Single(records);

        var record = records[0];

        Assert.Equal(0L, record[0]); //obsID
        Assert.Equal("Emil", record[1]); //Author
        Assert.Equal("Test", record[2]); //Observation
        Assert.Equal("Test Location", record[3]); //Location
        Assert.Equal(1234321L, record[4]); //Timestamp
    }

    const string resetDatabase = "DROP TABLE IF EXISTS ";
    public void Dispose()
    {
        using var connection = new SqliteConnection(
            $"Data Source={Path.Combine(DatabasePath, observationDatabase)}");

        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = resetDatabase+tableName+";";
        command.ExecuteNonQuery();

        connection.Close();
    }
}