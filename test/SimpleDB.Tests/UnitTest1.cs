using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace SimpleDB.Tests;

public class UnitTest1 : IDisposable
{
    private string filePath;
    private IDatabaseRepository<testRecord> testdb;
    private record testRecord(long id, string Author, string Observation, long time);

    public UnitTest1()
    {
        filePath = Path.Combine(AppContext.BaseDirectory,"../../../data/testDB.csv");
        testdb = new CSVDatabase<testRecord>("testDB");
    }

    [Fact]
    public void StoreRecordToEmptyDatabase()
    {
        Assert.False(File.Exists(filePath));

        var record = new testRecord(0, "me", "i saw a dog", 123);

        testdb.Store(record);

        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void ReadFromEmptyDatabase()
    {
        var records = testdb.Read();

        Assert.Empty(records);
    }

    [Fact]
    public void ReadObservationFromDatabase()
    {
        var record = new testRecord(0, "me", "i saw a Villads", 321);

        testdb.Store(record);

        var records = testdb.Read();

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
            File.Delete(filePath);
        }
    }
}