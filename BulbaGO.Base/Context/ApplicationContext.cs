using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulbaGO.Base.GeoIp;
using log4net;


namespace BulbaGO.Base.Context
{
    public static class ApplicationContext
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ApplicationContext));
        public static void Initialize()
        {
            Logger.Info("Initializing ApplicationContext");
            GeoIpHelper.Initialize();
            Logger.Info("Initialized Application Context");
        }
    }
}
