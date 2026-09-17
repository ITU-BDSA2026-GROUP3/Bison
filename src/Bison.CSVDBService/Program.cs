using SimpleDB;

var builder = WebApplication.CreateBuilder(args);
IDatabaseRepository database = CSVDatabase.Instance;
const string observationTableName = "bison_observe_cli_db";
const string commentTableName = "bison_comment_cli_db";

CSVDatabase.Instance.DirectoryPath = Path.GetFullPath(
    Path.Combine(AppContext.BaseDirectory, "../../../../../data"));
database.CreateTable<ObservationRec>(observationTableName);
database.CreateTable<CommentRec>(commentTableName);

var app = builder.Build();

app.MapPost("/comment/{id:long}", (long id, CommentRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Comment))
        return Results.BadRequest("Comment cannot be empty.");
    bool exists = database.Read<ObservationRec>(observationTableName).Any(obs => obs.obsID == id);

    if (!exists)
        return Results.NotFound();

    database.Store(commentTableName, new CommentRec(id, request.Comment));
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
public record CommentRequest(string Comment);
