using Bison.CLI;
using System;
using System.Collections;
using System.Collections.Generic;

public static class UserInterface
{
    public static void PrintObservations(IEnumerable<ObservationRec> obs)
    {
        if(obs.Count() == 0)
        {
            Console.WriteLine("No prior observations have been made.");
            Console.WriteLine("Please create an observation before reading.");
            return;
        }
        foreach (ObservationRec observation in obs)
        {
            DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(observation.Timestamp).ToLocalTime();

            string formattedDate = date.ToString("MM/dd/yy HH:mm:ss");

            Console.WriteLine($"{observation.Author} @ {formattedDate}: {observation.Observation}");
        }
    }

    public static void PrintComments(IEnumerable<CommentRec> comments)
    {
        foreach (CommentRec comment in comments)
        {
            Console.WriteLine(comment.Comment);
        }
    }

}
