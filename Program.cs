using System.IO;

namespace Bison.CLI
{
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