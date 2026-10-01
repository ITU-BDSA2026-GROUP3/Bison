using Microsoft.Data.Sqlite;

namespace Bison.Razor.Tests;

public class DatabaseHandler
{
    private static readonly string DatabasePath = Path.GetFullPath(
        Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../data"));
    
    public const string observationDatabase = "test_observe_cli.db";
    public const string observationTable = "test_observe_cli_db";
    public const string commentDatabase = "test_comment_cli.db";
    public const string commentTable = "test_comment_cli_db";
    public const string proposalDatabase = "test_proposal_cli.db";
    public const string proposalTable = "test_proposal_cli_db";
    
    public static void AddObservation(ObservationViewModel obs)
    {
        string commandText = 
        $"INSERT INTO {observationTable} (obsID, Author, Observation, Location, Timestamp) " +
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
        InsertIntoDatabase(commandText, parameters, observationDatabase);
    }

    public static void AddComment(CommentViewModel comment)
    {
        string commandText = 
        $"INSERT INTO {commentTable} (obsID, Comment) " +
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
        InsertIntoDatabase(commandText, parameters, commentDatabase);
    }

    /*public static void AddProposal(ProposalViewModel comment)
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

    public static void InitializeDatabases()
    {
        DBFacade.InitializeDatabase(
            observationDatabase,
            """
            CREATE TABLE IF NOT EXISTS test_observe_cli_db (
                obsID INTEGER PRIMARY KEY,
                Author TEXT NOT NULL,
                Observation TEXT NOT NULL,
                Location TEXT NOT NULL,
                Timestamp INTEGER NOT NULL
            );
            """);

        DBFacade.InitializeDatabase(
            commentDatabase,
            """
            CREATE TABLE IF NOT EXISTS test_comment_cli_db (
                obsID INTEGER NOT NULL,
                Comment TEXT NOT NULL
            );
            """);

        DBFacade.InitializeDatabase(
            proposalDatabase,
            """
            CREATE TABLE IF NOT EXISTS test_proposal_cli_db (
                obsID INTEGER NOT NULL,
                taxonID TEXT NOT NULL
            );
            """);
        
        DBFacade.observationDatabase = observationDatabase;
        DBFacade.observationTable = observationTable;
        DBFacade.commentDatabase = commentDatabase;
        DBFacade.commentTable = commentTable;
        DBFacade.proposalDatabase = proposalDatabase;
        DBFacade.proposalTable = proposalTable;
    }

    const string resetDatabase = "DROP TABLE IF EXISTS ";
    public static void ResetDatabases()
    {
        var connection = new SqliteConnection(
            $"Data Source={Path.Combine(DatabasePath, observationDatabase)}");

        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = resetDatabase+observationTable+";";
        command.ExecuteNonQuery();

        connection.Close();

        connection = new SqliteConnection(
            $"Data Source={Path.Combine(DatabasePath, commentDatabase)}");

        connection.Open();

        command = connection.CreateCommand();
        command.CommandText = resetDatabase+commentTable+";";
        command.ExecuteNonQuery();

        connection.Close();

        connection = new SqliteConnection(
            $"Data Source={Path.Combine(DatabasePath, proposalDatabase)}");

        connection.Open();

        command = connection.CreateCommand();
        command.CommandText = resetDatabase+proposalTable+";";
        command.ExecuteNonQuery();

        connection.Close();
    }
}