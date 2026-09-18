using SimpleDB;
using Bison.Taxonomy;
using System.Security.Cryptography.X509Certificates;
using System.Xml.XPath;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IDatabaseRepository,CSVDatabase>();

const string observationTableName = "bison_observe_cli_db";
const string commentTableName = "bison_comment_cli_db";
const string proposalTableName = "bison_proposal_cli_db";

var taxa = TaxonomyCsvLoader.Load();
ITaxonomyRepository taxonomyRepository = new TaxonomyRepository(taxa);


var app = builder.Build();
IDatabaseRepository? database = app.Services.GetService<IDatabaseRepository>();

database.DirectoryPath = Path.GetFullPath(
    Path.Combine(AppContext.BaseDirectory, "../../../../../data"));
database.CreateTable<ObservationRec>(observationTableName);
database.CreateTable<CommentRec>(commentTableName);
database.CreateTable<ProposalRec>(proposalTableName);

app.MapGet("/observations", () => database.Read<ObservationRec>(observationTableName));

app.MapGet("/comments", (long id) => database.Read<CommentRec>(commentTableName).Where(comment => comment.obsID == id)); // needs to only include comments that match key

app.MapGet("/proposals", () => database.Read<ProposalRec>(proposalTableName));


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

app.MapPost("/proposal", (ProposalRec proposalRec) =>
{
    bool observationExists = database
        .Read<ObservationRec>(observationTableName)
        .Any(obs => obs.obsID == proposalRec.obsID);

        bool taxonExists = taxonomyRepository.getByID(proposalRec.taxonID) is not null;

        //Treat the invalid observation/taxon like an invalid commenct target:
        //meaning do not store the proposal

        if (!observationExists || !taxonExists)
    {
        return Results.Ok();
    }

    database.Store<ProposalRec> (
        proposalTableName, proposalRec);


        return Results.Ok();
});










app.MapPost("/observation", (ObservationRec observation) =>
{
    if (string.IsNullOrWhiteSpace(observation.Author) || string.IsNullOrWhiteSpace(observation.Observation))
       
       
       {
         return Results.BadRequest("Author and Observation cannot be empty.");

       }


    database.Store(observationTableName, observation);
    return Results.Ok();
});

app.Run("http://localhost:5000");




public record ObservationRec(long obsID, string Author, string Observation, string Location, long Timestamp);
public record CommentRec(long obsID, string Comment);

public record ProposalRec(long obsID, string taxonID);