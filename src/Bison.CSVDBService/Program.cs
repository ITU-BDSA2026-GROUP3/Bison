using System.Security.Cryptography.X509Certificates;
using SimpleDB;

namespace Bison.CSVDBService
{
    public record ObservationRec(long obsID, string Author, string Observation, string Location, long Timestamp);
    public record CommentRec(long obsID, string Comment);

    public class Program
    {
        const string baseObservationTableName = "bison_observe_cli_db";
        const string baseCommentTableName = "bison_comment_cli_db";

        static string observationTableName = "";
        static string commentTableName = "";

        public static void Main(String[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSingleton<IDatabaseRepository,CSVDatabase>();
            
            
            var app = getApp(builder, args);

            app.Run("http://localhost:5000");
        }

        public static WebApplication getApp(WebApplicationBuilder builder, string[] args)
        {
            var app = builder.Build();
            IDatabaseRepository? database = app.Services.GetService<IDatabaseRepository>();

            observationTableName = (args.Count() == 0) ? baseObservationTableName : args[0].ToLower().Equals("test") ? "test_observe_cli_db" : baseObservationTableName;
            commentTableName = (args.Count() == 0) ? baseCommentTableName : args[0].ToLower().Equals("test") ? "test_comment_cli_db" : baseCommentTableName;

            database.DirectoryPath = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, "../../../../../data"));
            database.CreateTable<ObservationRec>(observationTableName);
            database.CreateTable<CommentRec>(commentTableName);

            app.MapGet("/observations", () => database.Read<ObservationRec>(observationTableName));

            app.MapGet("/comments", (long id) => database.Read<CommentRec>(commentTableName).Where(comment => comment.obsID == id)); // needs to only include comments that match key

            app.MapPost("/comment", (CommentRec commentRec) =>
            {
                foreach (ObservationRec obs in database.Read<ObservationRec>(observationTableName)) // ensures an observation with that id exists before adding comment
                {
                    if (obs.obsID == commentRec.obsID)
                    {
                        database.Store<CommentRec>(commentTableName, commentRec);
                        break;
                    }
                }
                return Results.Ok();
            });

            app.MapPost("/observation", (ObservationRec observation) =>
            {
                if (string.IsNullOrWhiteSpace(observation.Author) || string.IsNullOrWhiteSpace(observation.Observation))
                    return Results.BadRequest("Author and Observation cannot be empty.");

                database.Store(observationTableName, observation);
                return Results.Ok();
            });

            return app;
        }
    }
}