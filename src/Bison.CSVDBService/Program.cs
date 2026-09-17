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

app.MapPost("/observations/{id:long}/comments", (long id, CommentRequest request) =>
{
    bool exists = database.Read<ObservationRec>(observationTableName).Any(obs => obs.obsID == id);

    if (!exists)
        return Results.NotFound();

    database.Store(commentTableName, new CommentRec(id, request.Comment));
    return Results.Ok();
});

app.Run();

public record ObservationRec(long obsID, string Author, string Observation, long Timestamp);
public record CommentRec(long obsID, string Comment);
public record CommentRequest(string Comment);
