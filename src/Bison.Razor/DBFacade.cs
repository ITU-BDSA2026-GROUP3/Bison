using Microsoft.Data.Sqlite;

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

    public static string ReadComments() => ReadDatabase("bison_comment_cli.db", "bison_comment_cli_db");

    public static string ReadObservations() => ReadDatabase("bison_observe_cli.db", "bison_observe_cli_db");

    public static string ReadProposals() => ReadDatabase("bison_proposal_cli.db", "bison_proposal_cli_db");

    private static string ReadDatabase(string databaseFile, string tableName)
    {
        using var connection = new SqliteConnection(
            $"Data Source={Path.Combine(DatabasePath, databaseFile)}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {tableName}";
        List<string> returnedread = new List<string>();
        using var reader = command.ExecuteReader();
        
        while (reader.Read())
        {
            for (int column = 0; column < reader.FieldCount; column++)
            {
                returnedread.Add($"{reader.GetName(column)}: {reader.GetValue(column)}");
            }
        }

        return string.Join("\n", returnedread);
    }
}