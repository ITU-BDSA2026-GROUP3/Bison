using System.Data;
using Microsoft.Data.Sqlite;

var dataDirectory = Path.Combine(
    AppContext.BaseDirectory,
    "../../../../data"
);

var databaseFiles = new[]
{
    "bison_comment_cli.db",
    "bison_observe_cli.db",
    "bison_proposal_cli.db"
};

var BISONDBPATH = databaseFiles.All(file => File.Exists(Path.Combine(dataDirectory, file)))
    ? dataDirectory
    : Path.Combine(Path.GetTempPath(), "bison-db");

Directory.CreateDirectory(BISONDBPATH);

static string comments() {
    using (var connection = new SqliteConnection($"Data Source={Path.Combine(BISONDBPATH, "bison_comment_cli.db")}"))
    {
        connection.Open();
    }
}
static string observations() {
    using (var connection = new SqliteConnection($"Data Source={Path.Combine(BISONDBPATH, "bison_observe_cli.db")}"))
    {
        connection.Open();
    }
}
static string proposals() {
    using (var connection = new SqliteConnection($"Data Source={Path.Combine(BISONDBPATH, "bison_proposal_cli.db")}"))
    {
        connection.Open();
    }
}