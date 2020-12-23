using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Shodan.Net.Models
{
    /// <summary>
    /// result of <see cref="ShodanClient.RequestInternetPortScanAsync(int, string)"/>
    /// </summary>
    [DataContract]
    public class ScanPortResult
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
    }
}