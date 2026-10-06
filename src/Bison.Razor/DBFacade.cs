using Bison.Razor.Pages;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Data.Common;

public class DBFacade
{
    public string DatabaseName = "bison.db";

    public string DatabasePath;

    public DBFacade()
    {
        string? dbPath = Environment.GetEnvironmentVariable("BISONDBPATH");

        DatabasePath = string.IsNullOrWhiteSpace(dbPath) 
        ? Path.Combine(Path.Combine(
            AppContext.BaseDirectory,"../../../../../data"), "bison.db")
        :Path.GetFullPath(dbPath);

        Console.WriteLine("Path: "+DatabasePath);

        InitializeDatabases();
    }
    public void InitializeDatabases()
    {
        InitializeDatabase(
            """
            create table if not exists user (
            user_id integer primary key autoincrement,
            username string not null,
            email string not null,
            pw_hash string not null
            );
            """);

        InitializeDatabase(
            """
            create table if not exists observation (
            observation_id integer primary key autoincrement,
            author_id integer not null,
            text string not null,
            pub_date integer
            );
            """);
        
        InitializeDatabase(
            """
            create table if not exists comment (
            obsID integer not null,
            comment text not null);
            """);
        
        InitializeDatabase(
            """
            create table if not exists proposal (
            obsID integer not null,
            comment text not null);
            """);
    }

    public void InitializeDatabase(string commandText)
    {
        using var connection = new SqliteConnection(
            $"Data Source={Path.Combine(DatabasePath)}");

        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = commandText;
        command.ExecuteNonQuery();
        
    }

    public List<Object[]> ReadComments()
    {
        
        const string commandText = 
        " SELECT obsID, Comment FROM comment;";
        

        using var connection = new SqliteConnection($"Data Source = {DatabasePath}");

        connection.Open();

        using var command = connection.CreateCommand();

        

        command.CommandText = commandText;

        return ReadRows(command);


       
    }

    public List<Object[]> ReadObservations() => ReadDatabase("observation");

    public List<Object[]> ReadObservations(int page, int pageSize)
    {
        int validPage = Math.Max(page, 1);
        int validPageSize = Math.Max(pageSize, 1);
        int offset = (validPage - 1) * validPageSize;

        const string commandText =
            "SELECT observation.observation_id, user.username, " +
            "observation.text, observation.pub_date " +
            "FROM observation " +
            "JOIN user ON observation.author_id = user.user_id "+
            "ORDER BY observation.pub_date DESC, " +
            "observation.observation_id DESC "+
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

    public List<object[]> ReadObservationsFromAuthor(string author, int page, int pageSize)
    {
        int validPage = Math.Max(page, 1);
        int validPageSize = Math.Max(pageSize, 1);
        int offset = (validPage - 1) * validPageSize;

        const string commandText =
            "SELECT observation.observation_id, user.username, " +
            "observation.text, observation.pub_date " +
            "FROM observation " +
            "JOIN user ON observation.author_id = user.user_id "+
            "WHERE user.username = @author " +
            "ORDER BY observation.pub_date DESC, " +
            "observation.observation_id DESC "+
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

    public List<Object[]> ReadProposals()
    {

        const string commandText =
        "SELECT obsID, comment FROM proposal;";

        using var connection =  new SqliteConnection($"Data Source={DatabasePath}");

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = commandText;
        
        return ReadRows(command);
        
    }

    private List<object[]> ReadObservationDatabase(string commandText, Action<SqliteCommand> addParameters)
    {
        using var connection = new SqliteConnection($"Data Source={DatabasePath}");

        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = commandText;
        addParameters(command);

        return ReadRows(command);
    }

    public List<Object[]> ReadDatabase(string tableName)
    {
        using var connection = new SqliteConnection(
            $"Data Source={Path.Combine(DatabasePath)}");
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
    private List<object[]> ReadRows(SqliteCommand command)
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

    public object[]? ReadObservation(long id)
    {
        const string  commandText =
            "SELECT observation.observation_id, user.username, " +
            "observation.text, observation.pub_date " +
            "FROM observation " +
            "JOIN user ON observation.author_id = user.user_id "+
            "WHERE observation.observation_id = @id;";
          
            
          using var connection = new SqliteConnection($"Data Source={DatabasePath}");

            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = commandText;

            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            return null;
        }

        object[] row = new object[reader.FieldCount];
        reader.GetValues(row);

        return row;
   } 
}