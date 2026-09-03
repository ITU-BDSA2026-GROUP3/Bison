using System.CommandLine;
using System.IO;

namespace Bison.CLI
{
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
            StreamReader streamreader = new StreamReader("bison_observe_cli_db.csv");

            streamreader.ReadLine();

            while (streamreader.EndOfStream == false)
            {
                //Edit here
                String line = streamreader.ReadLine();

                int firstComma = line.IndexOf(',');
                int lastComma = line.LastIndexOf(',');

                string username = line.Substring(0, firstComma);
                string observation = line.Substring(firstComma + 1, lastComma - firstComma - 1);
                string unixTimestampStr = line.Substring(lastComma + 1);

                long unixTimestamp = long.Parse(unixTimestampStr);
                DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).ToLocalTime();
                string formattedDate = date.ToString("MM/dd/yy HH:mm:ss");
                
                Console.WriteLine($"{username} @ {formattedDate}: {observation}");            
                }
            streamreader.Close();
        }

        private static void WriteToCSV(string observation)
        {
            StreamWriter streamWriter = File.AppendText("bison_observe_cli_db.csv");
            streamWriter.WriteLine(Environment.UserName + "," + observation + "," + DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            streamWriter.Close();
        }

    }
}