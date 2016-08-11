using System.Linq;
using System.Net.NetworkInformation;

namespace BulbaGO.Base.Utils
{
    public static class IpTools
    {
        public static bool IsPortInUse(int port)
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            return ipGlobalProperties.GetActiveTcpListeners().Any(ep => ep.Port == port);
        }
    }
}
