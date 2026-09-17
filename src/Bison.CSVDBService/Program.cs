using SimpleDB;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IDatabaseRepository,CSVDatabase>();

const string observationTableName = "bison_observe_cli_db";
const string commentTableName = "bison_comment_cli_db";


var app = builder.Build();
IDatabaseRepository? database = app.Services.GetService<IDatabaseRepository>();

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

app.Run();




public record ObservationRec(long obsID, string Author, string Observation, long Timestamp);
public record CommentRec(long obsID, string Comment);