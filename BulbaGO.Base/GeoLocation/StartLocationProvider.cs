using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BulbaGO.Base.GeoLocation.Google;

namespace BulbaGO.Base.GeoLocation
{
    public static class StartLocationProvider
    {
        public static List<StartLocation> StartLocations = new List<StartLocation>();
        private static readonly Random LocationRandomizer = new Random();

        static StartLocationProvider()
        {
            StartLocations.Add(new StartLocation("US", "New Orleans Center", 29.971989, -90.093922, 3000));
            StartLocations.Add(new StartLocation("US", "Atlanta Downtown", 33.75628, -84.3911627, 1500));
            StartLocations.Add(new StartLocation("US", "Nashville", 36.155783, -86.7914057, 2000));
            StartLocations.Add(new StartLocation("US", "Kansas City Legoland", 39.081821, -94.5842627, 2000));
            StartLocations.Add(new StartLocation("US", "San Diego", 32.721531, -117.1595627, 2000));
            StartLocations.Add(new StartLocation("US", "Mineapolis Victory Memorial Park", 45.012586, -93.319527, 1500));
            StartLocations.Add(new StartLocation("US", "New York Central Park", 40.774901, -73.969585, 2000));
            StartLocations.Add(new StartLocation("US", "New York Empire State Building", 40.747848, -73.984806, 1000));
            StartLocations.Add(new StartLocation("AU", "Sydney CBD", -33.869047, 151.20952, 1000));
            StartLocations.Add(new StartLocation("AU", "The University of Sydney", -33.88574, 151.193362, 1500));
            StartLocations.Add(new StartLocation("AU", "Byron Bay", -28.648946, 153.613358, 1500));
        }

        public static async Task<StartLocation> GetRandomStartLocation(string twoLetterIsoCountryCode, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var startLocation = StartLocations.OrderBy(l => LocationRandomizer.Next()).FirstOrDefault();
            if (startLocation != null)
            {
                startLocation.Altitude = await Elevation.GetElevation(startLocation.Latitude, startLocation.Longitude, ct);
            }
            return startLocation;
        }
    }
}
