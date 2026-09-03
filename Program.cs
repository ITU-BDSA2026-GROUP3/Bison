using System.IO;
using System.Globalization;
using CsvHelper;

namespace Bison.CLI
{
    public record Cheep(string Author, string Message, long Timestamp);
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length > 0)
            {
                switch(args[0])
                {
                    case "read":
                        ReadFromCSV();

                        break;
                    case "observe":
                        if (args.Length > 1)
                        {
                            WriteToCSV(args[1]);
                        }
                        break;
                    default:
                        Console.WriteLine("command not recognized");
                        break;
                }
            }
        }

       private static void ReadFromCSV()
        {
            using var read = new StreamReader("bison_observe_cliv_db.csv");
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var cheeps = csv.GetRecord<Cheep>();

            foreach(var cheep in cheeps)
            {
                DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(cheep.Timestamp).ToLocalTime();
            }


            
        }

        private static void WriteToCSV(string observation)
        {
            StreamWriter streamWriter = File.AppendText("bison_observe_cli_db.csv");
            streamWriter.WriteLine(Environment.UserName + "," + observation + "," + DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            streamWriter.Close();
        }

    }
}