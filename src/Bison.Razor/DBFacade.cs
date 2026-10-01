using Microsoft.Data.Sqlite;
using System.Data;
using System.Data.Common;

public static class DBFacade
{

    private static readonly string DataDirectory = Path.GetFullPath(
        Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../data"));

    private static readonly string DatabasePath = DataDirectory;
    public static string observationDatabase = "bison_observe_cli.db";
    public static string observationTable = "bison_observe_cli_db";
    public static string commentDatabase = "bison_comment_cli.db";
    public static string commentTable = "bison_comment_cli_db";
    public static string proposalDatabase = "bison_proposal_cli.db";
    public static string proposalTable = "bison_proposal_cli_db";

    static DBFacade()
    {
        Directory.CreateDirectory(DatabasePath);
        InitializeDatabases();
    }

    private static void InitializeDatabases()
    {
        InitializeDatabase(
            "bison_observe_cli.db",
            """
            CREATE TABLE IF NOT EXISTS bison_observe_cli_db (
                obsID INTEGER PRIMARY KEY,
                Author TEXT NOT NULL,
                Observation TEXT NOT NULL,
                Location TEXT NOT NULL,
                Timestamp INTEGER NOT NULL
            );
            """);

        InitializeDatabase(
            "bison_comment_cli.db",
            """
            CREATE TABLE IF NOT EXISTS bison_comment_cli_db (
                obsID INTEGER NOT NULL,
                Comment TEXT NOT NULL
            );
            """);

        InitializeDatabase(
            "bison_proposal_cli.db",
            """
            CREATE TABLE IF NOT EXISTS bison_proposal_cli_db (
                obsID INTEGER NOT NULL,
                taxonID TEXT NOT NULL
            );
            """);
    }

    public static void InitializeDatabase(string databaseFile, string commandText)
    {
        using var connection = new SqliteConnection(
            $"Data Source={Path.Combine(DatabasePath, databaseFile)}");

        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = commandText;
        command.ExecuteNonQuery();
    }

    public static List<Object[]> ReadComments() => ReadDatabase(commentDatabase, commentTable);

    public static List<Object[]> ReadObservations() => ReadDatabase(observationDatabase, observationTable);

    public static List<Object[]> ReadObservations(int page, int pageSize)
    {
        int validPage = Math.Max(page, 1);
        int validPageSize = Math.Max(pageSize, 1);
        int offset = (validPage - 1) * validPageSize;

        string commandText =
            "SELECT obsID, Author, Observation, Location, Timestamp " +
            $"FROM {observationTable} " +
            "ORDER BY Timestamp DESC, obsID DESC " +
            "LIMIT @pageSize OFFSET @offset;";

        return ReadObservationDatabase(
            commandText,
            command =>
            {
                command.Parameters.AddWithValue(
                    "@pageSize",
                    validPageSize);

                command.Parameters.AddWithValue(
                    "@offset",
                    offset);
            });
    }

    public static List<object[]> ReadObservationsFromAuthor(string author, int page, int pageSize)
    {
        int validPage = Math.Max(page, 1);
        int validPageSize = Math.Max(pageSize, 1);
        int offset = (validPage - 1) * validPageSize;

        string commandText =
            "SELECT obsID, Author, Observation, Location, Timestamp " +
            $"FROM {observationTable} " +
            "WHERE Author = @author " +
            "ORDER BY Timestamp DESC, obsID DESC " +
            "LIMIT @pageSize OFFSET @offset;";

        return ReadObservationDatabase(
            commandText,
            command =>
            {
                command.Parameters.AddWithValue(
                    "@author",
                    author);

                command.Parameters.AddWithValue(
                    "@pageSize",
                    validPageSize);

                command.Parameters.AddWithValue(
                    "@offset",
                    offset);
            });
    }

    public static List<Object[]> ReadProposals() => ReadDatabase(proposalDatabase, proposalTable);

    private static List<object[]> ReadObservationDatabase(string commandText, Action<SqliteCommand> addParameters)
    {
        using var connection = new SqliteConnection($"Data Source={Path.Combine(DatabasePath, observationDatabase)}");

        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = commandText;
        addParameters(command);

        return ReadRows(command);
    }

    public static List<Object[]> ReadDatabase(string databaseFile, string tableName)
    {
        using var connection = new SqliteConnection(
            $"Data Source={Path.Combine(DatabasePath, databaseFile)}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {tableName}";
        List<Object[]> returnedread = new List<Object[]>();
        using var reader = command.ExecuteReader();
       
        while (reader.Read())
        {
            Object[] row = new Object[reader.FieldCount];

            reader.GetValues(row);
            returnedread.Add(row);
        }

        return returnedread;
    }
    private static List<object[]> ReadRows(SqliteCommand command)
    {
        List<object[]> rows = new();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            object[] row = new object[reader.FieldCount];
            reader.GetValues(row);
            rows.Add(row);
        }

        return rows;
    }
}