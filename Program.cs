using System.CommandLine;
using System.IO;
using System.Globalization;
using CsvHelper;
using SimpleDB;

namespace Bison.CLI
{
    public record ObservationRec(long obsID, string Author, string Observation, long Timestamp);
    public record CommentRec(long obsID, string Comment);
    class Program
    {
        
        static int Main(string[] args)
        {

            int IDcounter = 0; // temp solution
            IDatabaseRepository<ObservationRec> observationDatabase =
                new CSVDatabase<ObservationRec>();
            IDatabaseRepository<CommentRec> commentDatabase =
                new CSVDatabase<CommentRec>();

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

            Command observeCommand = new("observe", "Record a new observation.");

            observeCommand.Arguments.Add(observationArgument);

            observeCommand.SetAction(parseResult =>
            {
                string observation = parseResult.GetRequiredValue(observationArgument);
                WriteToCSV(observationDatabase, observation);
            });


            Argument<long> idArgument = new("id")
            {
                Description = "The id of the observation"
            };

            Command DiscussionCommand = new("discussion", "Read all comments for an observation.");

            DiscussionCommand.Arguments.Add(idArgument);

            DiscussionCommand.SetAction(parseResult =>
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
                commentDatabase.Store(new CommentRec(id,comment));
            });


            rootCommand.Subcommands.Add(readCommand);
            rootCommand.Subcommands.Add(observeCommand);
            rootCommand.Subcommands.Add(commentCommand);

            return rootCommand.Parse(args).Invoke();
        }

       private static void ReadFromCSV(IDatabaseRepository<ObservationRec> database)
        {
            IEnumerable<ObservationRec> cheeps = database.Read();

            UserInterface.PrintObservations(cheeps);


        }

        //WriteToCsv now uses CsvLibrary
        private static void WriteToCSV(IDatabaseRepository<ObservationRec> database, string observation)
        {
            var cheep = new ObservationRec(
                0, // placeholder
                Environment.UserName,
                observation, 
                DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            );

            database.Store(cheep);
        }
    }
}