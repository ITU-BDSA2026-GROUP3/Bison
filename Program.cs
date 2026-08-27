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
            while (streamreader.EndOfStream == false)
            {
                Console.WriteLine(streamreader.ReadLine());
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