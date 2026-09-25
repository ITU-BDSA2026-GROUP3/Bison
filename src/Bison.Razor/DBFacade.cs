using Microsoft.Data.Sqlite;
using System.Data;

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

    public static List<IDataRecord> ReadComments() => ReadDatabase("bison_comment_cli.db", "bison_comment_cli_db");

    public static List<IDataRecord> ReadObservations() => ReadDatabase("bison_observe_cli.db", "bison_observe_cli_db");

    public static List<IDataRecord> ReadProposals() => ReadDatabase("bison_proposal_cli.db", "bison_proposal_cli_db");

    private static List<IDataRecord> ReadDatabase(string databaseFile, string tableName)
    {
        using var connection = new SqliteConnection(
            $"Data Source={Path.Combine(DatabasePath, databaseFile)}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {tableName}";
        List<IDataRecord> returnedread = new List<IDataRecord>();
        using var reader = command.ExecuteReader();
       
        while (reader.Read())
        {
            returnedread.Add((IDataRecord)reader);
        }

        return returnedread;
    }
}