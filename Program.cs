<<<<<<< HEAD
﻿using System.IO;
using System.Globalization;
using CsvHelper;

namespace Bison.CLI
{
    public record Cheep(string Author, string Message, long Timestamp);
     public record Comment(string Author, string Message, long Timestamp);
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
            using var reader = new StreamReader("bison_observe_cli_db.csv");
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var cheeps = csv.GetRecords<Cheep>();

            foreach(var cheep in cheeps)
            {
                DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(cheep.Timestamp).ToLocalTime();

                string formattedDate = date.ToString("MM/dd/yy HH:mm:ss");

                Console.WriteLine($"{cheep.Author} @ {formattedDate}: {cheep.Message}");
            }
=======
﻿using System.CommandLine;
using System.IO;
using System.Globalization;
using CsvHelper;
using SimpleDB;
using System.ComponentModel;

namespace Bison.CLI
{
    public record ObservationRec(long obsID, string Author, string Observation, string Location, long Timestamp);
    public record CommentRec(long obsID, string Comment);
    class Program
    {
        
        static int Main(string[] args)
        {
            string observationFileName = "bison_observe_cli_db";
            string commentFileName = "bison_comment_cli_db";
            
            IDatabaseRepository<ObservationRec> observationDatabase =
                new CSVDatabase<ObservationRec>(observationFileName);
            IDatabaseRepository<CommentRec> commentDatabase =
                new CSVDatabase<CommentRec>(commentFileName);

            long IDcounter = GetIDSuccesor(observationDatabase); // temp solution

            RootCommand rootCommand = new("Bison CLI for recording and reading observations.");

            Command readCommand = new("read","Read all recorded observations.");

            readCommand.SetAction(_ =>
            {

                ReadFromCSV(observationDatabase);
            });

            Argument<string> observationArgument = new("observation")
            {
                Description = "The observation to record."
            };

            
            Argument<string> locationArgument = new("location")
            {
                Description = "The location of the observation."
            };


            Command observeCommand = new("observe", "Record a new observation.");

            observeCommand.Arguments.Add(observationArgument);
            observeCommand.Arguments.Add(locationArgument);

            observeCommand.SetAction(parseResult =>
            {
                string observation = parseResult.GetRequiredValue(observationArgument);
                string location = parseResult.GetRequiredValue(locationArgument);

               
                WriteToCSV(observationDatabase, observation, location, IDcounter);
            });



            Argument<long> idArgument = new("id")
            {
                Description = "The id of the observation"
            };

            Command discussionCommand = new("discussion", "Read all comments for an observation.");

            discussionCommand.Arguments.Add(idArgument);

            discussionCommand.SetAction(parseResult =>
            {
                long id = parseResult.GetRequiredValue(idArgument);

                UserInterface.PrintComments(
                    commentDatabase.Read()
                        .Where(comment => comment.obsID == id));
            });

            Argument<string> commentArgument = new("comment")
            {
                Description = "The comment to record."
            };

            Command commentCommand = new("comment", "Add a comment to an observation.");

            commentCommand.Arguments.Add(commentArgument);
            commentCommand.Arguments.Add(idArgument);

            commentCommand.SetAction(parseResult =>
            {
                string comment = parseResult.GetRequiredValue(commentArgument);
                long id = parseResult.GetRequiredValue(idArgument);
                foreach(ObservationRec obs in observationDatabase.Read()) // ensures an observation with that id exists before adding comment
                {
                    if(obs.obsID == id)
                    {
                        commentDatabase.Store(new CommentRec(id, comment));
                        break;
                    }
                }
            });


            rootCommand.Subcommands.Add(readCommand);
            rootCommand.Subcommands.Add(observeCommand);
            rootCommand.Subcommands.Add(discussionCommand);
            rootCommand.Subcommands.Add(commentCommand);

            return rootCommand.Parse(args).Invoke();
        }

       private static void ReadFromCSV(IDatabaseRepository<ObservationRec> database)
        {
            IEnumerable<ObservationRec> cheeps = database.Read();

            UserInterface.PrintObservations(cheeps);
>>>>>>> e784818 (The new parameter Location is added to the program and it works correctly,is tested, some syntax fixes done)


        }

        //WriteToCsv now uses CsvLibrary
<<<<<<< HEAD
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

=======
        private static void WriteToCSV(IDatabaseRepository<ObservationRec> database, string observation, string location, long id)
        {
            var cheep = new ObservationRec(
                id,
                Environment.UserName,
                observation, 
                location,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            );

            database.Store(cheep);
        }

        private static long GetIDSuccesor(IDatabaseRepository<ObservationRec> database)
        {
            var cheeps = database.Read();
            if(cheeps.Count() == 0) return 0;

            var cheep = cheeps.LastOrDefault();

            if(cheep == null) return 0;
            return cheep.obsID+1;
        }
>>>>>>> e784818 (The new parameter Location is added to the program and it works correctly,is tested, some syntax fixes done)
    }
}