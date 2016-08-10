using BulbaGO.Base.GeoLocation;
using log4net;


namespace BulbaGO.Base.Context
{
    public static class ApplicationContext
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ApplicationContext));
        public static void Initialize()
        {
            Logger.Debug("Initializing ApplicationContext");
            Countries.Initialize();
            GeoIpHelper.Initialize();
            Logger.Debug("Initialized Application Context");
        }
    }
}
