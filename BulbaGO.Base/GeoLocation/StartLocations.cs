using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulbaGO.Base.GeoLocation
{
    public class StartLocation
    {
        public string TwoLetterIsoCountryCode { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int MaxWalkDistance { get; set; }

        public StartLocation() { }

        public StartLocation(string twoLetterIsoCountryCode, string name, double latitude, double longitude, int maxWalkDistance)
        {
            TwoLetterIsoCountryCode = twoLetterIsoCountryCode;
            Name = name;
            Latitude = latitude;
            Longitude = longitude;
            MaxWalkDistance = maxWalkDistance;

        }
    }

    public static class StartLocationProvider
    {
        public static List<StartLocation> StartLocations = new List<StartLocation>();

        static StartLocationProvider()
        {
            StartLocations.Add(new StartLocation("US", "New Orleans Center", 29.971989, -90.093922, 3000));
            StartLocations.Add(new StartLocation("US", "Atlanta Downtown", 33.75628, -84.3911627, 1500));
            StartLocations.Add(new StartLocation("US", "Nashville", 36.155783, -86.7914057, 2000));
            StartLocations.Add(new StartLocation("US", "Kansas City Legoland", 39.081821, -94.5842627, 2000));
            StartLocations.Add(new StartLocation("US", "San Diego", 32.721531, -117.1595627, 2000));
        }

        public static StartLocation GetRandomStartLocation(string twoLetterIsoCountryCode)
        {
            return StartLocations.OrderBy(l => new Random().Next()).FirstOrDefault();
        }
    }
}
