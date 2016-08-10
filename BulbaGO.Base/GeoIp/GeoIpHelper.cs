using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Model;

namespace BulbaGO.Base.GeoIp
{
    public static class GeoIpHelper
    {
        private static DatabaseReader _geoIpDatabaseReader;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GeoIpHelper));

        public static void Initialize()
        {
            Logger.Info("Initializing GeoIp Reader");
            _geoIpDatabaseReader = new DatabaseReader(Path.Combine(Environment.CurrentDirectory, "GeoIp", "GeoLite2-Country.mmdb"));
            Logger.Info("Initialized GeoIp Reader");
        }

        private static Country _unknownCountry = new Country ();

        public static Country GetCountry(string ipAddress)
        {
            var response = _geoIpDatabaseReader.Country(ipAddress);
            if (response == null || response.Country == null)
            {
                return _unknownCountry;
            }
            return response.Country;
        }
    }
}
