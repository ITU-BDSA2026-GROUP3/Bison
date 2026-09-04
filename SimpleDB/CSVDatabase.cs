using System.CommandLine;
using System.IO;
using System.Globalization;
using CsvHelper;

namespace SimpleDB
{
    public sealed class CSVDatabase<T> : IDatabaseRepository<T> //Implements the IDatabaseRepository. Sealed means no class can inherit from CDVDatabase
    {
        public IEnumerable<T> Read(int? limit = null)
        { 
            bool fileExists = File.Exists(Path.Combine(AppContext.BaseDirectory, "../../../data/bison_observe_cli_db.csv"));

            if (!fileExists)
            {
                Console.WriteLine("No prior observations have been made.");
                Console.WriteLine("Please create an observation before reading.");
                return new List<T>();
            }

            using var reader = new StreamReader(Path.Combine(AppContext.BaseDirectory, "../../../data/bison_observe_cli_db.csv"));
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var cheeps = csv.GetRecords<T>().ToList();

            return cheeps;

        }
        public void Store(T record)
        {
            bool fileExists = File.Exists(Path.Combine(AppContext.BaseDirectory, "../../../data/bison_observe_cli_db.csv"));

            using var writer = new StreamWriter(Path.Combine(AppContext.BaseDirectory, "../../../data/bison_observe_cli_db.csv"), append: true);
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

