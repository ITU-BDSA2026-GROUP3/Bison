using System.CommandLine;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Xml.Linq;


namespace Bison.CLI
{
    public abstract record rec();
    public record ObservationRec(long obsID, string Author, string Observation, string Location, long Timestamp) : rec;
    public record CommentRec(long obsID, string Comment) : rec;
    public record ProposalRec(long obsID, string taxonID): rec;

    public class Program
    {
        static string baseURL = "http://localhost:5000"; //default is 5000
        static int Main(string[] args)
        {



            bool IDcounterRead = false;
            long IDcounter = 0; // temp solution

            RootCommand rootCommand = getRootCommands(IDcounter, IDcounterRead);

            return rootCommand.Parse(args).Invoke();
        }

        public static RootCommand getRootCommands(long IDcounter, bool IDcounterRead)
        {
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

            Argument<long> proposalIdArgument = new("id")
            {
                Description = "The id of the observation"
            };

            Argument<string> taxonIdArgument = new("taxonID")
            {
                Description = "The taxon ID being proposed."
            };

            Command proposalCommand = new ("propose", "Propose a taxon for an observation.");

            proposalCommand.Arguments.Add(proposalIdArgument);
            proposalCommand.Arguments.Add(taxonIdArgument);

                
            proposalCommand.SetAction(async parseResult =>
            {
                long id = parseResult.GetRequiredValue(proposalIdArgument);
                string taxonID = parseResult.GetRequiredValue(taxonIdArgument);

                await WriteProposalAsync(id, taxonID);
            });

            Command proposalsCommand = new("proposals","Read all taxon proposals.");

            proposalsCommand.SetAction(async _ =>
            {
                await ReadProposalsAsync();
                
            });

            rootCommand.Subcommands.Add(readCommand);
            rootCommand.Subcommands.Add(observeCommand);
            rootCommand.Subcommands.Add(discussionCommand);
            rootCommand.Subcommands.Add(commentCommand);
            rootCommand.Subcommands.Add(proposalCommand);
            rootCommand.Subcommands.Add(proposalsCommand);

            return rootCommand;
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
            
            var records = await client.GetFromJsonAsync<IEnumerable<ObservationRec>>("observations");

            ObservationRec obs = null;

            foreach(ObservationRec rec in records)
            {
                if(rec.obsID == id)
                {
                    obs = rec;
                    break;
                }
            }

            UserInterface.PrintObservation(obs);
            UserInterface.PrintComments(await client.GetFromJsonAsync<IEnumerable<CommentRec>>($"comments?id={id}"));
        }

        private static async Task WriteComment(string comment, long id)
        {
            using HttpClient client = new();
            client.BaseAddress = new Uri(baseURL);
            await client.PostAsJsonAsync("comment", new CommentRec(id, comment));
        }

        //here the new methods

        private static async Task WriteProposalAsync(long id, string taxonID)
        {
            using HttpClient client = new();
            client.BaseAddress = new Uri(baseURL);

            await client.PostAsJsonAsync("proposal", new ProposalRec(id, taxonID));

        }

        private static async Task ReadProposalsAsync()
        {
            using HttpClient client = new();
            client.BaseAddress = new Uri(baseURL);

            var proposals = await client.GetFromJsonAsync<IEnumerable<ProposalRec>>(
                "proposals");

                foreach (ProposalRec proposal in proposals ?? Enumerable.Empty<ProposalRec>())
            {
                Console.WriteLine($"Observation {proposal.obsID}: taxon {proposal.taxonID}");
                
            }
        }

        public static async Task<long> GetIDSuccesor()
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