using Microsoft.Data.Sqlite;

namespace Bison.Razor.Tests;

public class DatabaseHandler
{
    private static readonly string DatabasePath = Path.GetFullPath(
        Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../data"));
    
    
    public static void AddObservation(ObservationViewModel obs, string databaseFile, string tableName)
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
        InsertIntoDatabase(commandText, parameters, databaseFile);
    }

    public static void AddComment(CommentViewModel comment, string databaseFile, string tableName)
    {
        string commandText = 
        $"INSERT INTO {tableName} (obsID, Comment) " +
        "VALUES (@obsID, @Comment);";

        Action<SqliteCommand> parameters = command =>
        {
            command.Parameters.AddWithValue(
                "@obsID", comment.obsID
            );
            command.Parameters.AddWithValue(
                "@Comment", comment.Comment
            );
        };
        InsertIntoDatabase(commandText, parameters, databaseFile);
    }

    /*public static void AddProposal(ProposalViewModel comment, string databaseFile, string tableName)
    {
        
    }*/

    private static void InsertIntoDatabase(string commandText, Action<SqliteCommand> parameters, string databaseFile)
    {
        using var connection = new SqliteConnection($"Data Source={Path.Combine(DatabasePath, databaseFile)}");

        connection.Open();
        
        using var command = connection.CreateCommand();
        command.CommandText = commandText;
        parameters(command);

        command.ExecuteNonQuery();

        connection.Close();
    }
}