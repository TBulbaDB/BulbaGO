using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BulbaGO.Base.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BulbaGO.Base.GeoLocation.Google
{
    public static class Elevation
    {
        private const string ApiKey = "AIzaSyBtSBYXIWuuPEAcWBMcC6pbS5Um1gjJdgc";
        private const string UrlFormat = "https://maps.googleapis.com/maps/api/elevation/json?locations={0},{1}&key={2}";

        public static async Task<double> GetElevation(double latitude, double longitude, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var elevation = new Random().NextDouble() * 10;
            try
            {
                var uri = new Uri(string.Format(UrlFormat, latitude, longitude, ApiKey));
                var client = SuperHttpClient.GetInstance();
                var result = await client.GetStringAsync(uri);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    var resultObject = JObject.Parse(result);
                    if (resultObject != null)
                    {
                        return (double)resultObject.SelectTokens("$..elevation").FirstOrDefault();
                    }
                }
            }
            catch (Exception)
            {

            }

            return elevation;
        }
    }
}
