using Microsoft.Data.Sqlite;
using System.Data;
using System.Data.Common;

public static class DBFacade
{
    private static readonly string DataDirectory = Path.Combine(
        AppContext.BaseDirectory,
        "../../../../data");

    private static readonly string[] DatabaseFiles =
    {
        "bison_comment_cli.db",
        "bison_observe_cli.db",
        "bison_proposal_cli.db"
    };

    private static readonly string DatabasePath = DatabaseFiles.All(file => File.Exists(Path.Combine(DataDirectory, file))) ? DataDirectory : Path.Combine(Path.GetTempPath(), "bison-db");

    static DBFacade()
    {
        Directory.CreateDirectory(DatabasePath);
    }

    public static List<Object[]> ReadComments() => ReadDatabase("bison_comment_cli.db", "bison_comment_cli_db");

    public static List<Object[]> ReadObservations() => ReadDatabase("bison_observe_cli.db", "bison_observe_cli_db");

    public static List<Object[]> ReadObservations(int page, int pageSize)
    {
        int validPage = Math.Max(page, 1);
        int validPageSize = Math.Max(pageSize, 1);
        int offset = (validPage - 1) * validPageSize;

        const string commandText =
            "SELECT obsID, Author, Observation, Location, Timestamp " +
            "FROM bison_observe_cli_db " +
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

        const string commandText =
            "SELECT obsID, Author, Observation, Location, Timestamp " +
            "FROM bison_observe_cli_db " +
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

    public static List<Object[]> ReadProposals() => ReadDatabase("bison_proposal_cli.db", "bison_proposal_cli_db");

    private static List<object[]> ReadObservationDatabase(string commandText, Action<SqliteCommand> addParameters)
    {
        using var connection = new SqliteConnection($"Data Source={Path.Combine(DatabasePath, "bison_observe_cli.db")}");

        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = commandText;
        addParameters(command);

        return ReadRows(command);
    }

    private static List<Object[]> ReadDatabase(string databaseFile, string tableName)
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