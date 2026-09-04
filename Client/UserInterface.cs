using Bison.CLI;
using System;
using System.Collections;
using System.Collections.Generic;

public static class UserInterface
{
    public static void PrintObservations(IEnumerable<Cheep> obs)
    {
        foreach (Cheep cheep in obs)
        {
            DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(cheep.Timestamp).ToLocalTime();

            string formattedDate = date.ToString("MM/dd/yy HH:mm:ss");

            Console.WriteLine($"{cheep.Author} @ {formattedDate}: {cheep.Observation}");
        }

    }

}
