using System.Collections.Generic;
using System.Linq;

namespace Shodan.Net.Standard.Search
{
    public class FacetGenerator
    {
        private readonly Dictionary<string, int?> queryData = new Dictionary<string, int?>();

        internal FacetGenerator()
        {
        }

        internal string GenerateFacets()
        {
            var data = queryData.Select(a => a.Value.HasValue ? $"{a.Key}:{a.Value}" : a.Key);
            return string.Join(",", data);
        }

        public FacetGenerator WithAsn(int? count = null)
        {
            queryData.Add("asn", count);
            return this;
        }

        public FacetGenerator WithCity(int? count = null)
        {
            queryData.Add("city", count);
            return this;
        }

        public FacetGenerator WithCountry(int? count = null)
        {
            queryData.Add("country", count);
            return this;
        }

        public FacetGenerator WithDevice(int? count = null)
        {
            queryData.Add("device", count);
            return this;
        }

        public FacetGenerator WithDomain(int? count = null)
        {
            queryData.Add("domain", count);
            return this;
        }

        public FacetGenerator WithGeocluster(int? count = null)
        {
            queryData.Add("geocluster", count);
            return this;
        }

        public FacetGenerator HasScreenshot(int? count = null)
        {
            queryData.Add("has_screenshot", count);
            return this;
        }

        public FacetGenerator WithIsp(int? count = null)
        {
            queryData.Add("isp", count);
            return this;
        }

        public FacetGenerator WithLink(int? count = null)
        {
            queryData.Add("link", count);
            return this;
        }

        public FacetGenerator WithOrg(int? count = null)
        {
            queryData.Add("org", count);
            return this;
        }

        public FacetGenerator WithOs(int? count = null)
        {
            queryData.Add("os", count);
            return this;
        }

        public FacetGenerator WithPort(int? count = null)
        {
            queryData.Add("port", count);
            return this;
        }

        public FacetGenerator WithPostal(int? count = null)
        {
            queryData.Add("postal", count);
            return this;
        }

        public FacetGenerator WithState(int? count = null)
        {
            queryData.Add("state", count);
            return this;
        }

        public FacetGenerator WithTimestampDay(int? count = null)
        {
            queryData.Add("timestamp_day", count);
            return this;
        }

        public FacetGenerator WithTimestampMonth(int? count = null)
        {
            queryData.Add("timestamp_month", count);
            return this;
        }

        public FacetGenerator WithTimestampYear(int? count = null)
        {
            queryData.Add("timestamp_year", count);
            return this;
        }

        public FacetGenerator WithUptime(int? count = null)
        {
            queryData.Add("uptime", count);
            return this;
        }

        public FacetGenerator WithVersion(int? count = null)
        {
            queryData.Add("version", count);
            return this;
        }

        public FacetGenerator WithBitcoinIp(int? count = null)
        {
            queryData.Add("bitcoin.ip", count);
            return this;
        }

        public FacetGenerator WithBitcoinIpCount(int? count = null)
        {
            queryData.Add("bitcoin.ip_count", count);
            return this;
        }

        public FacetGenerator WithBitcoinPort(int? count = null)
        {
            queryData.Add("bitcoin.port", count);
            return this;
        }

        public FacetGenerator WithBitcoinUserAgent(int? count = null)
        {
            queryData.Add("bitcoin.user_agent", count);
            return this;
        }

        public FacetGenerator WithBitcoinVersion(int? count = null)
        {
            queryData.Add("bitcoin.version", count);
            return this;
        }

        public FacetGenerator WithNtpIp(int? count = null)
        {
            queryData.Add("ntp.ip", count);
            return this;
        }

        public FacetGenerator WithNtpIpCount(int? count = null)
        {
            queryData.Add("ntp.ip_count", count);
            return this;
        }

        public FacetGenerator WithNtpMore(int? count = null)
        {
            queryData.Add("ntp.more", count);
            return this;
        }

        public FacetGenerator WithNtpPort(int? count = null)
        {
            queryData.Add("ntp.port", count);
            return this;
        }

        public FacetGenerator WithSshCipher(int? count = null)
        {
            queryData.Add("ssh.cipher", count);
            return this;
        }

        public FacetGenerator WithSshFingerprint(int? count = null)
        {
            queryData.Add("ssh.fingerprint", count);
            return this;
        }

        public FacetGenerator WithSshMac(int? count = null)
        {
            queryData.Add("ssh.mac", count);
            return this;
        }

        public FacetGenerator WithSshType(int? count = null)
        {
            queryData.Add("ssh.type", count);
            return this;
        }
    }
}