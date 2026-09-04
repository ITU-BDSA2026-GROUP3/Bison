using System.CommandLine;
using System.IO;
using System.Globalization;
using CsvHelper;

namespace Bison.CLI
{
    public record Cheep(string Author, string Observation, long Timestamp);
    class Program
    {
        static int Main(string[] args)
        {
            RootCommand rootCommand = new("Bison CLI for recording and reading observations.");

            Command readCommand = new("read","Read all recorded observations.");

            readCommand.SetAction(_ =>
            {
                ReadFromCSV();
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
                WriteToCSV(observation);
            });

            rootCommand.Subcommands.Add(readCommand);
            rootCommand.Subcommands.Add(observeCommand);

            return rootCommand.Parse(args).Invoke();
        }

       private static void ReadFromCSV()
        {
            using var reader = new StreamReader("bison_observe_cli_db.csv");
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var cheeps = csv.GetRecords<Cheep>();

            UserInterface.PrintObservations(cheeps);


        }

        //WriteToCsv now uses CsvLibrary
        private static void WriteToCSV(string observation)
        {
            var cheep = new Cheep(
                Environment.UserName,
                observation, 
                DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            );

            bool fileExists = File.Exists("bison_observe_cli_db.csv");

            using var writer = new StreamWriter("bison_observe_cli_db.csv", append: true);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            if(!fileExists)
            {
                csv.WriteHeader<Cheep>();
                csv.NextRecord();
            }

            csv.WriteRecord(cheep);
            csv.NextRecord();



        }

    }
}