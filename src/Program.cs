using SimpleDB;
using System.CommandLine;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace Bison.CLI
{
    public abstract record rec();
    public record ObservationRec(long obsID, string Author, string Observation, long Timestamp) : rec;
    public record CommentRec(long obsID, string Comment) : rec;
    class Program
    {
        static readonly IDatabaseRepository database = CSVDatabase.Instance;

        const string observationTableName = "bison_observe_cli_db";

        const string commentTableName = "bison_comment_cli_db";
        static int Main(string[] args)
        {
            CSVDatabase.Instance.DirectoryPath = Path.Combine(AppContext.BaseDirectory, "data"); // lazy solution to set directory path"

            database.CreateTable<ObservationRec>(observationTableName);
            database.CreateTable<CommentRec>(commentTableName);

            long IDcounter = GetIDSuccesor(); // temp solution

            RootCommand rootCommand = new("Bison CLI for recording and reading observations.");

            Command readCommand = new("read","Read all recorded observations.");

            readCommand.SetAction(_ =>
            {
                ReadObservations();
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
                WriteObservation(observation,IDcounter);
            });


            Argument<long> idArgument = new("id")
            {
                Description = "The id of the observation"
            };

            Command discussionCommand = new("discussion", "Read all comments for an observation.");

            discussionCommand.Arguments.Add(idArgument);

            discussionCommand.SetAction(parseResult =>
            {
                ReadComments(parseResult.GetRequiredValue(idArgument));
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
                WriteComment(comment, id);
     
            });


            rootCommand.Subcommands.Add(readCommand);
            rootCommand.Subcommands.Add(observeCommand);
            rootCommand.Subcommands.Add(discussionCommand);
            rootCommand.Subcommands.Add(commentCommand);

            return rootCommand.Parse(args).Invoke();
        }

       private static void ReadObservations()
        {
            IEnumerable<ObservationRec> cheeps = database.Read<ObservationRec>(observationTableName);

            UserInterface.PrintObservations(cheeps);


        }

        private static void WriteObservation(string observation, long id)
        {
            var cheep = new ObservationRec(
                id,
                Environment.UserName,
                observation, 
                DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            );

            database.Store<ObservationRec>(observationTableName,cheep);
        }

        private static void ReadComments(long id)
        {
            UserInterface.PrintComments(
                    database.Read<CommentRec>(commentTableName)
                        .Where(comment => comment.obsID == id));
        }

        private static void WriteComment(string comment, long id)
        {
            foreach (ObservationRec obs in database.Read<ObservationRec>(observationTableName)) // ensures an observation with that id exists before adding comment
            {
                if (obs.obsID == id)
                {
                    database.Store<CommentRec>(commentTableName, new CommentRec(id, comment));
                    break;
                }
            }
        }

        private static long GetIDSuccesor()
        {
            var cheeps = database.Read<ObservationRec>(observationTableName);
            if(cheeps.Count() == 0) return 0;

            var cheep = cheeps.LastOrDefault();

            if(cheep == null) return 0;
            return cheep.obsID+1;
        }
    }
}