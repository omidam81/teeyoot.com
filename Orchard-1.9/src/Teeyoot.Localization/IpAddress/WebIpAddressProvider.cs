using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Teeyoot.Localization.IpAddress
{
    public class WebIpAddressProvider : IIpAddressProvider
    {
        public string GetIpAddress()
        {
            try
            {
                var userHostAddress = HttpContext.Current.Request.UserHostAddress;

                // Attempt to parse.  If it fails, we catch below and return 0.0.0.0
                // ReSharper disable once AssignNullToNotNullAttribute
                IPAddress.Parse(userHostAddress);

                var forwarded = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(forwarded) || forwarded.ToLowerInvariant() == "unknown")
                    return userHostAddress;

                var forwardedIpAddresses = forwarded.Split(',');
                var publicForwardedIpAddresses = forwardedIpAddresses.Where(ip => !IsPrivateIpAddress(ip)).ToList();

                return publicForwardedIpAddresses.Any() ? publicForwardedIpAddresses.Last() : userHostAddress;
            }
            catch (Exception)
            {
                LogErrors();
                return "0.0.0.0";
            }
        }

        private static bool IsPrivateIpAddress(string ipAddress)
        {
            var ip = IPAddress.Parse(ipAddress);
            var octets = ip.GetAddressBytes();

            var is24BitBlock = octets[0] == 10;
            if (is24BitBlock)
                return true;

            var is20BitBlock = octets[0] == 172 && octets[1] >= 16 && octets[1] <= 31;
            if (is20BitBlock)
                return true;

            var is16BitBlock = octets[0] == 192 && octets[1] == 168;
            if (is16BitBlock)
                return true;

            var isLinkLocalAddress = octets[0] == 169 && octets[1] == 254;
            return isLinkLocalAddress;
        }

        private static void LogErrors()
        {
            const string logsDirectoryRelativePath = "/App_Data/Logs";

            var logsDirectoryPhysicalPath = HttpContext.Current.Server.MapPath(logsDirectoryRelativePath);

            File.AppendAllText(Path.Combine(logsDirectoryPhysicalPath, "WebIpAddressProviderLogs.txt"),
                string.Format("Date: {0} \r\nUserHostAddress: {1} \r\nForwarded: {2}\r\n\r\n",
                    DateTime.UtcNow,
                    HttpContext.Current.Request.UserHostAddress,
                    HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"]));
        }
    }
}
