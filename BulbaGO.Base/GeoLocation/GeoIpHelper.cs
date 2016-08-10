using System;
using System.IO;
using log4net;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Model;
using MaxMind.GeoIP2.Responses;

namespace BulbaGO.Base.GeoLocation
{
    public static class GeoIpHelper
    {
        private static DatabaseReader _geoIpDatabaseReader;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GeoIpHelper));

        public static void Initialize()
        {
            Logger.Debug("Initializing GeoIp Reader");
            _geoIpDatabaseReader = new DatabaseReader(Path.Combine(Environment.CurrentDirectory, "GeoLocation", "GeoLite2-Country.mmdb"));
            Logger.Debug("Initialized GeoIp Reader");
        }

        private static readonly Country UnknownCountry = new Country();

        public static Country GetCountry(string ipAddress)
        {
            try
            {
                CountryResponse countryResponse;
                if (_geoIpDatabaseReader.TryCountry(ipAddress, out countryResponse))
                {
                    return countryResponse.Country ?? UnknownCountry;
                }
                return UnknownCountry;
            }
            catch (Exception)
            {
                return UnknownCountry;
            }

        }
    }
}
