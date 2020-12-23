using System;
using System.Collections.Generic;
using System.Text;

namespace Shodan.Net.Standard.Search
{
    public class QueryGenerator
    {
        internal Dictionary<string, string> QueryData = new Dictionary<string, string>();
        private readonly StringBuilder searchTextBuilder;

        internal QueryGenerator()
        {
            searchTextBuilder = new StringBuilder();
        }

        public QueryGenerator Before(DateTime time)
        {
            QueryData.Add("before", time.ToString("dd/MM/yyyy"));
            return this;
        }

        /// <summary>
        /// Only show results that were collected after the given date
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public QueryGenerator After(DateTime time)
        {
            QueryData.Add("after", time.ToString("dd/MM/yyyy"));
            return this;
        }

        /// <summary>
        /// The Autonomous System Number that identifies the network the device is on.
        /// </summary>
        /// <param name="asn"></param>
        /// <returns></returns>
        public QueryGenerator WithAsn(string asn)
        {
            QueryData.Add(nameof(asn), asn);
            return this;
        }

        public QueryGenerator WithCity(string city)
        {
            QueryData.Add(nameof(city), city);
            return this;
        }

        public QueryGenerator WithCountry(string country)
        {
            QueryData.Add(nameof(country), country);
            return this;
        }

        public QueryGenerator WithState(string state)
        {
            QueryData.Add(nameof(state), state);
            return this;
        }

        /// <summary>
        /// If "true" only show results that have a screenshot available.
        /// </summary>
        /// <param name="hasScreenshot"></param>
        /// <returns></returns>
        public QueryGenerator HasScreenshot(bool hasScreenshot = true)
        {
            QueryData.Add("has_screenshot", hasScreenshot.ToString());
            return this;
        }

        public QueryGenerator WithHostname(string hostname)
        {
            QueryData.Add(nameof(hostname), hostname);
            return this;
        }

        public QueryGenerator WithHtml(string html)
        {
            QueryData.Add(nameof(html), html);
            return this;
        }

        public QueryGenerator WithIsp(string isp)
        {
            QueryData.Add(nameof(isp), isp);
            return this;
        }

        public QueryGenerator WithNet(string net)
        {
            QueryData.Add(nameof(net), net);
            return this;
        }

        public QueryGenerator WithOrg(string org)
        {
            QueryData.Add(nameof(org), org);
            return this;
        }

        public QueryGenerator WithOs(string os)
        {
            QueryData.Add(nameof(os), os);
            return this;
        }

        public QueryGenerator WithPort(int port)
        {
            QueryData.Add(nameof(port), port.ToString());
            return this;
        }

        public QueryGenerator WithPostal(string postal)
        {
            QueryData.Add(nameof(postal), postal);
            return this;
        }

        public QueryGenerator WithProduct(string product)
        {
            QueryData.Add(nameof(product), product);
            return this;
        }

        public QueryGenerator WithTitle(string title)
        {
            QueryData.Add(nameof(title), title);
            return this;
        }

        public QueryGenerator WithVersion(string version)
        {
            QueryData.Add(nameof(version), version);
            return this;
        }

        public QueryGenerator WithBitcoinIp(string bitcoinIp)
        {
            QueryData.Add("bitcoin.ip", bitcoinIp);
            return this;
        }

        public QueryGenerator WithBitcoinIpCount(string bitcoinIpCount)
        {
            QueryData.Add("bitcoin.ip_count", bitcoinIpCount);
            return this;
        }

        public QueryGenerator WithBitcoinPort(string bitcoinPort)
        {
            QueryData.Add("bitcoin.port", bitcoinPort);
            return this;
        }

        public QueryGenerator WithBitcoinVersion(string bitcoinVersion)
        {
            QueryData.Add("bitcoin.version", bitcoinVersion);
            return this;
        }

        public QueryGenerator WithNtpIp(string ntpIp)
        {
            QueryData.Add("ntp.ip", ntpIp);
            return this;
        }

        public QueryGenerator WithNtpIpCount(string ntpIpCount)
        {
            QueryData.Add("ntp.ip_count", ntpIpCount);
            return this;
        }

        public QueryGenerator WithNtpMore(string ntpMore)
        {
            QueryData.Add("ntp.more", ntpMore);
            return this;
        }

        public QueryGenerator WithNtpPort(int ntpPort)
        {
            QueryData.Add("ntp.port", ntpPort.ToString());
            return this;
        }

        public QueryGenerator SearchTerm(string searchTerm)
        {
            if (searchTextBuilder.Length != 0)
            {
                throw new ShodanException("Method cannot be called twice");
            }

            searchTextBuilder.Append(searchTerm);
            return this;
        }

        internal string Generate()
        {
            foreach(var item in QueryData)
            {
                searchTextBuilder.Append($" {item.Key}:{item.Value}");
            }

            return searchTextBuilder.ToString();
        }
    }
}