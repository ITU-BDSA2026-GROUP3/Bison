using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace SimpleDB.Tests;

[Collection("Sequential Tests")]
public class UnitTest1 : IDisposable, IClassFixture<DatabaseFixture>
{
    private string filePath;
    private string tablename;
    private IDatabaseRepository testdb;
    private record testRecord(long id, string Author, string Observation, long time);

    public UnitTest1()
    {
        testdb = CSVDatabase.Instance;
        CSVDatabase.Instance.DirectoryPath = Path.Combine(AppContext.BaseDirectory, "../../../data");
        tablename = "test table";
        filePath = Path.Combine(AppContext.BaseDirectory, $"../../../data/{tablename}.csv");
    }

    [Fact]
    public void StoreRecordToEmptyDatabase()
    {
        Assert.False(File.Exists(filePath));

        var record = new testRecord(0, "me", "i saw a dog", 123);

        testdb.CreateTable<testRecord>(tablename);
        testdb.Store(tablename, record);

        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void ReadFromEmptyDatabase()
    {
        testdb.CreateTable<testRecord>(tablename);
        var records = testdb.Read<testRecord>(tablename);

        Assert.Empty(records);
    }

    [Fact]
    public void ReadObservationFromDatabase()
    {
        var record = new testRecord(0, "me", "i saw a Villads", 321);

        testdb.CreateTable<testRecord>(tablename);
        testdb.Store<testRecord>(tablename, record);

        var records = testdb.Read<testRecord>(tablename);

        Assert.Single(records);

        var storedRecord = records.LastOrDefault();

        Assert.NotNull(storedRecord);
        Assert.Equal(record.id, storedRecord.id);
        Assert.Equal(record.Author, storedRecord.Author);
        Assert.Equal(record.Observation, storedRecord.Observation);
        Assert.Equal(record.time, storedRecord.time);
    }

    public void Dispose()
    {
        if (File.Exists(filePath))
        {
            File.WriteAllText(filePath, string.Empty);
        }
    }
}

public class DatabaseFixture : IDisposable
{
    public string FilePath { get; }

    public DatabaseFixture()
    {
        string tablename = "test table";
        FilePath = Path.Combine(AppContext.BaseDirectory, $"../../../data/{tablename}.csv");
    }

    public void Dispose()
    {
        // Sletter filen helt fra harddisken, når ALLE tests i klassen er færdige
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
    }
}