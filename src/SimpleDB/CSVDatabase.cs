using System.CommandLine;
using System.IO;
using System.Globalization;
using CsvHelper;

namespace SimpleDB
{
    public sealed class CSVDatabase<T> : IDatabaseRepository<T> //Implements the IDatabaseRepository. Sealed means no class can inherit from CDVDatabase
    {
        string CSVfilePath;
        public CSVDatabase(string CSVfileName)
        {
            CSVfilePath = Path.Combine(AppContext.BaseDirectory, $"../../../data/{CSVfileName}.csv");
        }
        
        public IEnumerable<T> Read(int? limit = null)
        { 
            bool fileExists = File.Exists(CSVfilePath);

            if (!fileExists)
            {
                Console.WriteLine("No prior observations have been made.");
                Console.WriteLine("Please create an observation before reading.");
                return new List<T>();
            }

            using var reader = new StreamReader(CSVfilePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var cheeps = csv.GetRecords<T>().ToList();

            return cheeps;

        }
        public void Store(T record)
        {
            bool fileExists = File.Exists(CSVfilePath);

            using var writer = new StreamWriter(CSVfilePath, append: true);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            if(!fileExists)
            {
                csv.WriteHeader<T>();
                csv.NextRecord();
            }

            csv.WriteRecord(record);
            csv.NextRecord();
        }
    }
}

