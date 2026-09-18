using System.CommandLine;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Xml.Linq;
using Bison.Taxonomy;

namespace Bison.CLI
{
    public abstract record rec();
    public record ObservationRec(long obsID, string Author, string Observation, string Location, long Timestamp) : rec;
    public record CommentRec(long obsID, string Comment) : rec;
    class Program
    {
        static string baseURL = "http://localhost:5000"; //default is 5000
        static int Main(string[] args)
        {


            var taxa = TaxonomyCsvLoader.Load();
            ITaxonomyRepository taxonomyRepository = new TaxonomyRepository(taxa);

            bool IDcounterRead = false;
            long IDcounter = 0; // temp solution

            RootCommand rootCommand = new("Bison CLI for recording and reading observations.");

            Command readCommand = new("read", "Read all recorded observations.");

            readCommand.SetAction(async _ =>
            {
                await ReadObservationsAsync();
            });

            Argument<string> observationArgument = new("observation")
            {
                Description = "The observation to record."
            };

            Argument<string>  locationArgument = new("location")
            {
                Description = "The location where the observation was made."
            };

            Command observeCommand = new("observe", "Record a new observation.");

            observeCommand.Arguments.Add(observationArgument);
            observeCommand.Arguments.Add(locationArgument);

            observeCommand.SetAction(async parseResult =>
            {
                if (!IDcounterRead)
                {
                    IDcounter = await GetIDSuccesor();
                    IDcounterRead = true;
                }
                string observation = parseResult.GetRequiredValue(observationArgument);
                string location = parseResult.GetRequiredValue(locationArgument);

                await WriteObservationAsync(observation,location, IDcounter);
            });


            Argument<long> idArgument = new("id")
            {
                Description = "The id of the observation"
            };

            Command discussionCommand = new("discussion", "Read all comments for an observation.");

            discussionCommand.Arguments.Add(idArgument);

            discussionCommand.SetAction(async parseResult =>
            {
                await ReadComments(parseResult.GetRequiredValue(idArgument));
            });

            Argument<string> commentArgument = new("comment")
            {
                Description = "The comment to record."
            };

            Command commentCommand = new("comment", "Add a comment to an observation.");

            commentCommand.Arguments.Add(commentArgument);
            commentCommand.Arguments.Add(idArgument);

            commentCommand.SetAction(async parseResult =>
            {
                string comment = parseResult.GetRequiredValue(commentArgument);
                long id = parseResult.GetRequiredValue(idArgument);
                await WriteComment(comment, id);

            });


            rootCommand.Subcommands.Add(readCommand);
            rootCommand.Subcommands.Add(observeCommand);
            rootCommand.Subcommands.Add(discussionCommand);
            rootCommand.Subcommands.Add(commentCommand);

            return rootCommand.Parse(args).Invoke();
        }

        private static async Task ReadObservationsAsync()
        {
            using HttpClient client = new();
            client.BaseAddress = new Uri(baseURL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            UserInterface.PrintObservations(await client.GetFromJsonAsync<IEnumerable<ObservationRec>>($"observations"));


        }

        private static async Task WriteObservationAsync(string observation, string location, long id)
        {
            var cheep = new ObservationRec(
                id,
                Environment.UserName,
                observation, 
                location,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            );
            using HttpClient client = new();
            client.BaseAddress = new Uri(baseURL);
            await client.PostAsJsonAsync("observation", cheep);

        }

        private static async Task ReadComments(long id)
        {
            using HttpClient client = new();
            client.BaseAddress = new Uri(baseURL);

            UserInterface.PrintComments(await client.GetFromJsonAsync<IEnumerable<CommentRec>>($"comments?id={id}"));
        }

        private static async Task WriteComment(string comment, long id)
        {
            using HttpClient client = new();
            client.BaseAddress = new Uri(baseURL);
            await client.PostAsJsonAsync("comment", new CommentRec(id, comment));
        }

        private static async Task<long> GetIDSuccesor()
        {
            using HttpClient client = new();
            client.BaseAddress = new Uri(baseURL);

            var cheeps = await client.GetFromJsonAsync<IEnumerable<ObservationRec>>("observations");
            if (cheeps.Count() == 0) return 0;

            var cheep = cheeps.LastOrDefault();

            if (cheep == null) return 0;
            return cheep.obsID + 1;
        }
    }
}