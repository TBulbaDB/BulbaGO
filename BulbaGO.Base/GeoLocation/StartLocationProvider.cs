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
