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
        AddToDatabase(new ObservationViewModel(0, "Emil", "Test", "Test Location", "1234321"));

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

    private void AddToDatabase(ObservationViewModel obs)
    {
        string commandText = 
        $"INSERT INTO {tableName} (obsID, Author, Observation, Location, Timestamp) " +
        "VALUES (@obsID, @Author, @Observation, @Location, @Timestamp);";

        Action<SqliteCommand> parameters = command =>
        {
            command.Parameters.AddWithValue(
                "@obsID", obs.obsID
            );
            command.Parameters.AddWithValue(
                "@Author", obs.Author
            );
            command.Parameters.AddWithValue(
                "@Observation", obs.Message
            );
            command.Parameters.AddWithValue(
                "@Location", obs.Location
            );
            command.Parameters.AddWithValue(
                "@Timestamp", obs.Timestamp
            );
        };
        
        using var connection = new SqliteConnection($"Data Source={Path.Combine(DatabasePath, "test_observe_cli.db")}");

        connection.Open();
        
        using var command = connection.CreateCommand();
        command.CommandText = commandText;
        parameters(command);

        command.ExecuteNonQuery();

        connection.Close();
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