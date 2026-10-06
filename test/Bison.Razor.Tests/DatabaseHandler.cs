using Microsoft.Data.Sqlite;

namespace Bison.Razor.Tests;

public class DatabaseHandler
{
    private static readonly string DatabasePath = Path.GetFullPath(
        Path.Combine(Path.Combine(
            AppContext.BaseDirectory,"../../../../../data"), "test.db"));
    
    
    public static void AddObservation(ObservationViewModel obs, int author_id)
    {
        const string commandText = 
        "INSERT INTO observation (observation_id, author_id, text, pub_date) " +
        "VALUES (@obsID, @Author_id, @Observation, @Timestamp);";

        Action<SqliteCommand> parameters = command =>
        {
            command.Parameters.AddWithValue(
                "@obsID", obs.obsID
            );
            command.Parameters.AddWithValue(
                "@Author_id", author_id
            );
            command.Parameters.AddWithValue(
                "@Observation", obs.Message
            );
            command.Parameters.AddWithValue(
                "@Timestamp", obs.Timestamp
            );
        };
        InsertIntoDatabase(commandText, parameters);
    }

    public static void AddUser(int user_id, string username, string email, string pw_hash)
    {
        const string commandText = 
        "INSERT INTO user (user_id, username, email, pw_hash) " +
        "VALUES (@user_id, @username, @email, @pw_hash);";

        Action<SqliteCommand> parameters = command =>
        {
            command.Parameters.AddWithValue(
                "@user_id", user_id
            );
            command.Parameters.AddWithValue(
                "@username", username
            );
            command.Parameters.AddWithValue(
                "@email", email
            );
            command.Parameters.AddWithValue(
                "@pw_hash", pw_hash
            );
        };
        InsertIntoDatabase(commandText, parameters);
    }

    public static void AddComment(CommentViewModel comment)
    {
        string commandText = 
        "INSERT INTO comment (obsID, Comment) " +
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
        InsertIntoDatabase(commandText, parameters);
    }

    public static void AddProposal(ProposalViewModel comment)
    {
        string commandText = 
        "INSERT INTO comment (obsID, Comment) " +
        "VALUES (@obsID, @Comment);";

        Action<SqliteCommand> parameters = command =>
        {
            command.Parameters.AddWithValue(
                "@obsID", comment.obsID
            );
            command.Parameters.AddWithValue(
                "@Comment", comment.taxonID
            );
        };
        InsertIntoDatabase(commandText, parameters);
    }

    private static void InsertIntoDatabase(string commandText, Action<SqliteCommand> parameters)
    {
        using var connection = new SqliteConnection($"Data Source={Path.Combine(DatabasePath)}");

        connection.Open();
        
        using var command = connection.CreateCommand();
        command.CommandText = commandText;
        parameters(command);

        command.ExecuteNonQuery();

        connection.Close();
    }

    public static void InitializeDatabases(DBFacade db)
    {
        db.DatabasePath = Path.Combine(Path.Combine(
            AppContext.BaseDirectory,"../../../../../data"), "test.db");
        db.InitializeDatabases();
    }

    const string resetDatabase = "DROP TABLE IF EXISTS ";
    public static void ResetDatabases()
    {
        var connection = new SqliteConnection(
            $"Data Source={Path.Combine(DatabasePath)}");

        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = resetDatabase+"observation;";
        command.ExecuteNonQuery();

        command = connection.CreateCommand();
        command.CommandText = resetDatabase+"user;";
        command.ExecuteNonQuery();

        command = connection.CreateCommand();
        command.CommandText = resetDatabase+"comment;";
        command.ExecuteNonQuery();

        command = connection.CreateCommand();
        command.CommandText = resetDatabase+"proposal;";
        command.ExecuteNonQuery();

        connection.Close();
    }
}