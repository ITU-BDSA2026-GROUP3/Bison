using Bison.CLI;
using System;
using System.Collections;
using System.Collections.Generic;

public static class UserInterface
{
    public static void PrintObservations(IEnumerable<ObservationRec> obs)
    {
        foreach (ObservationRec observation in obs)
        {
            DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(observation.Timestamp).ToLocalTime();

            string formattedDate = date.ToString("MM/dd/yy HH:mm:ss");

            Console.WriteLine($"{observation.Author} @ {formattedDate}: {observation.Observation}");
        }

    }

}
