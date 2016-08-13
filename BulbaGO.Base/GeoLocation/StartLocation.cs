namespace BulbaGO.Base.GeoLocation
{
    public class StartLocation
    {
        public string TwoLetterIsoCountryCode { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
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
}