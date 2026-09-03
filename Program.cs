using System.CommandLine;
using System.IO;
using System.Globalization;
using CsvHelper;
using SimpleDB;

namespace Bison.CLI
{
    public record Cheep(string Author, string Message, long Timestamp);
    class Program
    {
        
        static int Main(string[] args)
        {
            IDatabaseRepository<Cheep> database = new CSVDatabase<Cheep>();

            RootCommand rootCommand = new("Bison CLI for recording and reading observations.");

            Command readCommand = new("read","Read all recorded observations.");

            readCommand.SetAction(_ =>
            {
                ReadFromCSV(database);
            });

            Argument<string> observationArgument = new("observation")
            {
                Description = "The observation to record."
            };

            Command observeCommand = new("observe", "Record a new observation.");

            observeCommand.Arguments.Add(observationArgument);

            observeCommand.SetAction(parseResult =>
            {
                string observation = parseResult.GetRequiredValue(observationArgument);
                WriteToCSV(database, observation);
            });

            rootCommand.Subcommands.Add(readCommand);
            rootCommand.Subcommands.Add(observeCommand);

            return rootCommand.Parse(args).Invoke();
        }

       private static void ReadFromCSV(IDatabaseRepository<Cheep> database)
        {
            IEnumerable<Cheep> cheeps = database.Read();

            foreach(var cheep in cheeps)
            {
                DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(cheep.Timestamp).ToLocalTime();

                string formattedDate = date.ToString("MM/dd/yy HH:mm:ss");

                Console.WriteLine($"{cheep.Author} @ {formattedDate}: {cheep.Message}");
            }
        }

        //WriteToCsv now uses CsvLibrary
        private static void WriteToCSV(IDatabaseRepository<Cheep> database, string observation)
        {
            var cheep = new Cheep(
                Environment.UserName,
                observation, 
                DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            );

            database.Store(cheep);
        }
    }
}