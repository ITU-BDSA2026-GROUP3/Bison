
using System.Net.Http.Json;
using System.Numerics;
using System.Runtime.InteropServices;
using Bison.Taxonomy;
using Microsoft.AspNetCore.Builder;

namespace Bison.CLI.Tests;

[Collection("Sequential Tests")]
public class FuzzEndToEndTests : IDisposable, IClassFixture<WebAppFixture>
{
    private string obsFilePath;
    private string comFilePath;

    private HttpClient client;
    private WebApplication app;

    private string name;
    private List<long> validIDs;
    private ITaxonomyRepository taxonomyRepository;

    private const string validTaxonID = "MSTSNM:Arter:3e4e67e4-f785-ea11-aa77-501ac539d1ea";
    private const string badUglyStupidNotValidTaxonID = "This is NOT a valid ID and should NEVER work >:)";

    private List<(ObservationRec,string)> expectedObservations;
    private List<(CommentRec,string)> expectedComments;
    private List<(ProposalRec,string)> expectedProposals;

    Random random = new Random();

    public FuzzEndToEndTests(WebAppFixture fixture)
    {
        obsFilePath = fixture.obsFilePath;
        comFilePath = fixture.comFilePath;

        name = Environment.UserName;
        
        app = fixture.App;
        client = fixture.Client;

        validIDs = new List<long>();
        var taxa = TaxonomyCsvLoader.Load();
        taxonomyRepository = new TaxonomyRepository(taxa);

        expectedObservations = new List<(ObservationRec,string)>();
        expectedComments = new List<(CommentRec,string)>();
        expectedProposals = new List<(ProposalRec, string)>();

        ResetCSVFiles();
    }

    [Fact]
    public async Task FuzzTesting()
    {
        int testCases = 100;

        Console.WriteLine($"----- RUNNING {testCases} FUZZ TESTS");

        for(int i=0; i<testCases; i++)
        {
            switch (random.Next(3))
            {
                case 0:
                    await FuzzObservation();
                    break;
                case 1:
                    await FuzzComment();
                    break;
                case 2:
                    await FuzzProposal();
                    break;
            }
        }
        
        await ValidateOracle();
    }

    private async Task FuzzObservation()
    {
        string observation = "Valid Observation Message";
        string location = "Current Location";

        int mutations = random.Next(5,100);

        for(int i=0; i<mutations; i++)
        {
            int pick = random.Next(2);

            if(pick == 0) observation = await Mutate(observation);
            else location = await Mutate(location);
        }

        var rootCommands = Program.getRootCommands(0, false);

        var ID = await Program.GetIDSuccesor();
        
        int exitcode = await rootCommands.Parse(new[] {"observe", observation, location}).InvokeAsync();

        if(exitcode == 0)
        {
            expectedObservations.Add((new ObservationRec(ID, name, observation, location, DateTimeOffset.UtcNow.ToUnixTimeSeconds()), $"observe {observation} {location}"));
            validIDs.Add(ID);
        }
    }

    private async Task FuzzComment()
    {
        string comment = "Cool Observation dude";
        long obsID = (random.Next(10) == 0 || validIDs.Count() == 0) ? random.NextInt64(long.MinValue, long.MaxValue) : validIDs[random.Next(validIDs.Count)];

        int mutations = random.Next(5, 50);

        for(int i=0; i<mutations; i++)
        {
            comment = await Mutate(comment);
        }

        var rootCommands = Program.getRootCommands(0, false);

        int exitcode = await rootCommands.Parse(new[] {"comment", comment, obsID.ToString()}).InvokeAsync();

        if(validIDs.Contains(obsID))
        {
            expectedComments.Add((new CommentRec(obsID, comment), $"comment {comment} {obsID}"));
        }
    }

    private async Task FuzzProposal()
    {
        string taxonID = (random.Next(2) == 0) ? validTaxonID : badUglyStupidNotValidTaxonID;
        long obsID = (random.Next(10) == 0 || validIDs.Count() == 0) ? random.NextInt64(long.MinValue, long.MaxValue) : validIDs[random.Next(validIDs.Count())];

        var rootCommands = Program.getRootCommands(0,false);

        int exitcode = await rootCommands.Parse(new[] {"propose", obsID.ToString(), taxonID}).InvokeAsync();

        if(taxonomyRepository.GetById(taxonID) != null && validIDs.Contains(obsID))
        {
            expectedProposals.Add((new ProposalRec(obsID, taxonID), $"propose {obsID} {taxonID}"));
        }
    }

    private async Task ValidateOracle()
    {
        var rawObservations = await client.GetFromJsonAsync<IEnumerable<ObservationRec>>("observations");
        List<ObservationRec> actualObservations = rawObservations.OrderBy(o => o.obsID).ToList();

        var sortedExpectedObservations = expectedObservations.OrderBy(e => e.Item1.obsID).ToList();

        Console.WriteLine("----- STARTING VALIDATION OF FUZZ TESTS");

        if(actualObservations.Count() != expectedObservations.Count())
        {
            Console.WriteLine($"[ERROR] Expected Observations Count doesn't match Actual Observations Count. Expected {expectedObservations.Count()}, Actual {actualObservations.Count()}");
        }

        int comparedItems = Math.Min(sortedExpectedObservations.Count(), actualObservations.Count());

        for(int i=0; i<comparedItems; i++)
        {
            Console.WriteLine($"----- VALIDATING OBSERVATION {i}");
            
            ObservationRec expected = sortedExpectedObservations[i].Item1;
            ObservationRec actual = actualObservations[i];

            if(expected.obsID != actual.obsID || !expected.Observation.Equals(actual.Observation) || !expected.Location.Equals(actual.Location))
            {
                Console.WriteLine($"[ERROR] Expected Observation doesn't match Actual Observation."); 

                Console.WriteLine($"Command: {sortedExpectedObservations[i].Item2}");

                Console.WriteLine("Expected (Oracle):");
                UserInterface.PrintObservation(expected);

                Console.WriteLine("Actual (Database):");
                UserInterface.PrintObservation(actual);
            }

            if (!expected.Observation.Equals(actual.Observation))
            {
                Console.WriteLine($"[ERROR] Expected Observation doesn't match Actual Observation. Expected {expected.Observation}, Actual {actual.Observation}");
            }
            
            await ValidateComments(expected.obsID);
        }

        await ValidateProposals();
    }

    private async Task ValidateComments(long ID)
    {
        var rawCommentsForObs = await client.GetFromJsonAsync<IEnumerable<CommentRec>>($"comments?id={ID}");
        List<CommentRec> actualCommentsForObs = rawCommentsForObs.ToList();

        var expectedCommentsForObs = expectedComments.FindAll(c => c.Item1.obsID == ID);

        if(actualCommentsForObs.Count() == 0 && expectedCommentsForObs.Count() == 0) return;

        Console.WriteLine($"----- VALIDATING COMMENTS");
        
        if(actualCommentsForObs.Count() != expectedCommentsForObs.Count())
        {
            Console.WriteLine($"[ERROR] Expected Comments Count doesn't match Actual Comments Count. Expected {expectedCommentsForObs.Count()}, Actual {actualCommentsForObs.Count()}");
        }

        int comparedComments = Math.Min(actualCommentsForObs.Count(), expectedCommentsForObs.Count());

        for(int j=0; j<comparedComments; j++)
        {
            CommentRec expectedComment = expectedCommentsForObs[j].Item1;
            CommentRec actualComment = actualCommentsForObs[j];

            if (!expectedComment.Comment.Equals(actualComment.Comment))
            {
                Console.WriteLine($"[ERROR] Expected Comment doesn't match Actual Comment."); 

                Console.WriteLine($"Command: {expectedCommentsForObs[j].Item2}");

                Console.WriteLine($"Expected '{expectedComment.Comment}', Actual '{actualComment.Comment}'");
            }
        }
    }

    private async Task ValidateProposals()
    {
        var rawProposalsForObs = await client.GetFromJsonAsync<IEnumerable<ProposalRec>>($"proposals");
        List<ProposalRec> actualProposals = rawProposalsForObs.ToList();
        actualProposals.Sort((a1, a2)  => a1.obsID.CompareTo(a2.obsID));

        var sortedExpectedProposals = expectedProposals.OrderBy(e => e.Item1.obsID).ToList();

        Console.WriteLine("----- VALIDATING PROPOSALS");

        if(sortedExpectedProposals.Count() != actualProposals.Count()){
            Console.WriteLine($"[ERROR] Expected Proposals Count doesn't match Actual Proposals Count. Expected {expectedProposals.Count()}, Actual {actualProposals.Count()}");
        }

        int comparedItems = Math.Min(sortedExpectedProposals.Count(), actualProposals.Count());

        for(int i=0; i<comparedItems; i++)
        {
            Console.WriteLine($"----- VALIDATING PROPOSAL {i}");
            
            ProposalRec expected = sortedExpectedProposals[i].Item1;
            ProposalRec actual = actualProposals[i];

            if(!expected.taxonID.Equals(actual.taxonID) || expected.obsID != actual.obsID)
            {
                Console.WriteLine("[ERROR] Expected Proposal doens't match Actual Proposal.");

                Console.WriteLine($"Command: {sortedExpectedProposals[i].Item2}");

                Console.WriteLine($"Expected TaxonID: '{expected.taxonID}'");
                Console.WriteLine($"Expected ObservationID: '{expected.obsID}'");

                Console.WriteLine($"Actual TaxonID: '{actual.taxonID}'");
                Console.WriteLine($"Actual ObservationID: '{actual.obsID}'");
            }
        }
    }


    //Mutation methods
    private async Task<string> Mutate(string input)
    {
        int mutationType = random.Next(4);
        switch (mutationType)
        {
            case 0:
                return await Add(input);
            case 1:
                return await Delete(input);
            case 2:
                return await Swap(input);
            case 3:
                return await Replace(input);
            default:
                return input;
        }
    }

    private async Task<string> Add(string input)
    {
        int index = random.Next(input.Length);
        char randomChar = (char)random.Next(32, 126);
        return input.Insert(index, randomChar.ToString());
    }

    private async Task<string> Delete(string input)
    {
        int index = random.Next(input.Length);
        return input.Remove(index, 1);
    }

    private async Task<string> Swap(string input)
    {
        int index1 = random.Next(input.Length);
        int index2 = random.Next(input.Length);
        char[] ch = input.ToCharArray();
        char temp = ch[index1];
        ch[index1] = ch[index2];
        ch[index2] = temp;
        return new string(ch);
    }

    private async Task<string> Replace(string input)
    {
        int index = random.Next(input.Length);
        char randomChar = (char)random.Next(32, 126);
        char[] ch = input.ToCharArray();
        ch[index] = randomChar;
        return new string(ch);
    }

    //File handling
    private void ResetCSVFiles()
    {
        if (File.Exists(obsFilePath))
        {
            File.WriteAllText(obsFilePath, string.Empty);
        }

        if (File.Exists(comFilePath))
        {
            File.WriteAllText(comFilePath, string.Empty);
        }
    }

    public void Dispose()
    {
        ResetCSVFiles();
    }
}