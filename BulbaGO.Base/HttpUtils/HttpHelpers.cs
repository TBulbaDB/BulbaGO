using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using log4net;

namespace BulbaGO.Base.HttpUtils
{
    public static class HttpHelpers
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HttpHelpers));

        public static async Task<string> WhatIsMyIp(IWebProxy proxy = null)
        {
            try
            {
                const string checkUrl = "http://prch.checkoid.com/index.ashx";
                var client = SuperHttpClient.GetInstance(proxy);
                var response = await client.GetAsync(checkUrl);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var clientHeaders = GetWhatIsMyIpHeaders(responseContent);
                if (clientHeaders.ContainsKey("REMOTE_ADDR"))
                {
                    return clientHeaders["REMOTE_ADDR"];
                }
            }
            catch (HttpRequestException hre)
            {
                Logger.Error($"[{hre.GetType().Name}] {hre.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[{ex.GetType().Name}] {ex.Message}");
            }

            return null;
        }

        private static Dictionary<string, string> GetWhatIsMyIpHeaders(string checkResult)
        {
            var result = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(checkResult)) return result;
            var myIpResulArray = checkResult.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in myIpResulArray)
            {
                var arr = s.Split(new[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length == 2 && !result.ContainsKey(arr[0]))
                {
                    result.Add(arr[0], arr[1]);
                }
            }
            return result;
        }
    }
}
