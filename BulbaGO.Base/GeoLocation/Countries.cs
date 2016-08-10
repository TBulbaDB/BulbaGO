using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;

namespace BulbaGO.Base.GeoLocation
{
    public static class Countries
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Countries));

        public static List<RegionInfo> All { get; private set; }
        public static Dictionary<string, RegionInfo> Iso2 { get; private set; }
        public static Dictionary<string, RegionInfo> Iso3 { get; private set; }


        public static void Initialize()
        {
            Logger.Debug("Initializing Countries");
            All = new List<RegionInfo>();
            foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                var country = new RegionInfo(culture.LCID);
                if (All.All(p => p.Name != country.Name))
                    All.Add(country);
            }
            All = All.OrderBy(c => c.EnglishName).ToList();
            Iso2 = All.ToDictionary(r => r.TwoLetterISORegionName);
            Iso3 = All.ToDictionary(r => r.TwoLetterISORegionName);
            Logger.Debug("Initialized Countries");

        }
    }
}
