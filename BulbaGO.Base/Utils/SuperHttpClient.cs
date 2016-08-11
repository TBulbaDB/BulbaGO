using System.Net;
using System.Net.Http;

namespace BulbaGO.Base.Utils
{
    public class SuperHttpClient : HttpClient
    {
        protected SuperHttpClient()
        {
        }

        protected SuperHttpClient(HttpMessageHandler handler) : base(handler)
        {
        }

        protected SuperHttpClient(HttpMessageHandler handler, bool disposeHandler) : base(handler, disposeHandler)
        {
        }

        protected static HttpMessageHandler ProxiedHandler(IWebProxy proxy)
        {
            return new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = false,
                UseProxy = proxy != null,
                Proxy = proxy
            };
        }

        public static SuperHttpClient GetInstance()
        {
            return new SuperHttpClient();
        }

        public static SuperHttpClient GetInstance(IWebProxy proxy)
        {
            return new SuperHttpClient(ProxiedHandler(proxy));
        }

    }
}
