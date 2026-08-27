using System.IO;

namespace Bison.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Environment.UserName);
            if(args.Length > 0)
            {
                switch(args[0])
                {
                    case "read":
                        ReadFromCSV();

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

    }
}