using System.Collections.Generic;
using CommandLine;

namespace CameraCollector.ConsoleApp
{
    public class RuntimeParameters
    {
        [Option('m', "mode", HelpText = "Search or Import", Required = true, Default = "search")]
        public string Mode { get; set; }

        [Option('c', "camera-type", HelpText = "Name of camera brand to scan for", Required = false, Default = "foscam")]
        public string CameraType { get; set; }

        [Option('k', "api-key", HelpText = "Shodan.io API Key", Required = false)]
        public string ApiKey { get; set; }

        [Option('p', "ports", HelpText = "Comma-delimited list of ports you want to scan.", Required = false, Default = null)]
        public string PortListString { get; set; }  //TODO:Implement this in ScannerConfiguration & CameraScanner

        [Option('P', "pages", HelpText = "Number of Shodan results pages to retrieve. If port list is specified, multiply pages by ports to determine # of API Requests", Required = false, Default = 1)]
        public int Pages { get; set; }

        [Option('n', "country", HelpText = "Country code (ex. US) in which you would like to target your search", Required = false)]
        public string Country { get; set; }

        [Option('i', "input", HelpText = "Shodan.io export file", Required = false)]
        public string InputFile { get; set; }

        [Option('l', "log", HelpText = "Log cameras to flat text file", Required = false)]
        public string LogFile { get; set; }

        [Option('t', "threads", HelpText = "Number of threads to scan with.", Required = false, Default = 5)]
        public int Threads { get; set; }

        [Option('A', "adjacent-ports", HelpText = "Number of adjacent ports to crawl", Required = false, Default = 0)]
        public int AdjacentPorts { get; set; }

        [Option('C', "common", HelpText = "Indicates desire to crawl common Foscam ports", Required = false, Default = false)]
        public bool CommonPorts { get; set; }

        [Option('O', "optimistic-crawl", HelpText = "Indicates desire to enqueue adjacent and common ports only prior to attempting connection", Required = false, Default = false)]
        public bool PessimisticCrawl { get; set; }
    }
}