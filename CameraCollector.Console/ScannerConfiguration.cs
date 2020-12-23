using CommandLine;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace CameraCollector.ConsoleApp
{
    public class ScannerConfiguration
    {
        public ScanMode ScanMode { get; private set; }
        public string CameraTypeName { get; private set; }
        public string ApiKey { get; private set; }
        public List<int> Ports { get; private set; }
        public int Pages { get; private set; }
        public string Country { get; private set; }
        public FileInfo InputFile { get; private set; }
        public FileInfo LogFile { get; private set; }
        public int Threads { get; private set; }
        public int AdjacentPortDepth { get; private set; }
        public bool CrawlCommonPorts { get; private set; }
        public bool OptimisticCrawl { get; private set; }

        public ScannerConfiguration(IEnumerable<string> args)
        {

            var parser = new Parser(p =>
            {
                p.CaseSensitive = true;
                p.AutoHelp = true;
                p.HelpWriter = Console.Error;
            });

            parser.ParseArguments<RuntimeParameters>(args)
                .WithParsed(SetConfigOptions)
                .WithNotParsed(Program.ReportError);

            ValidateConfiguration();
        }

        private void SetConfigOptions(RuntimeParameters parameters)
        {
            switch (parameters.Mode.ToLowerInvariant())
            {
                case "search":
                {
                    ScanMode = ScanMode.Search;
                    CameraTypeName = parameters.CameraType;
                    Country = parameters.Country;
                    Pages = parameters.Pages;
                    
                    if (!string.IsNullOrWhiteSpace(parameters.LogFile))
                        LogFile = new FileInfo(parameters.LogFile);

                    var shodanApiKey = Program.Configuration.GetValue<string>("ShodanApiKey");
                    ApiKey = !string.IsNullOrWhiteSpace(parameters.ApiKey) ? parameters.ApiKey : shodanApiKey;

                    break;
                }
                case "import":
                {
                    ScanMode = ScanMode.Import;
                    CameraTypeName = parameters.CameraType;
                    InputFile = new FileInfo(parameters.InputFile);

                    if (!string.IsNullOrWhiteSpace(parameters.LogFile))
                    {
                        if (!string.IsNullOrWhiteSpace(parameters.LogFile))
                        {
                            if (parameters.LogFile.Contains('/') || parameters.LogFile.Contains('\\'))
                                LogFile = new FileInfo(parameters.LogFile);
                            else
                                LogFile = parameters.LogFile.ToLowerInvariant().EndsWith(".txt") ?
                                    new FileInfo($"{InputFile.DirectoryName}\\{parameters.LogFile}") :
                                    new FileInfo($"{InputFile.DirectoryName}\\{parameters.LogFile}.txt");
                        }
                        else
                        {
                            LogFile = new FileInfo($"{InputFile.DirectoryName}\\sh-output.txt");
                        }
                    }

                    break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(ScanMode), parameters.Mode, "Invalid scan mode specified.");
            }

            if (!string.IsNullOrWhiteSpace(parameters.PortListString))
            {
                Ports = new List<int>();

                foreach (var portString in parameters.PortListString.Split(','))
                {
                    try
                    {
                        Ports.Add(Convert.ToInt32(portString.Trim()));
                    }
                    catch
                    {
                        throw new ArgumentOutOfRangeException(nameof(Ports), portString, "This is not a valid port number");
                    }
                }
            }

            Threads = parameters.Threads;
            AdjacentPortDepth = parameters.AdjacentPorts;
            CrawlCommonPorts = parameters.CommonPorts;
            OptimisticCrawl = parameters.PessimisticCrawl;
        }

        private void ValidateConfiguration()
        {
            if (Threads < 1)
                throw new ArgumentOutOfRangeException(nameof(Threads), Threads, "Number of threads cannot be less than 1.");

            if (AdjacentPortDepth < 0)
                throw new ArgumentOutOfRangeException(nameof(AdjacentPortDepth), AdjacentPortDepth, "Adjacent port depth cannot be negative.");

            if (string.IsNullOrWhiteSpace(CameraTypeName))
                throw new ArgumentOutOfRangeException(nameof(CameraTypeName), CameraTypeName, "Camera type must be specified.");

            switch (ScanMode)
            {
                case ScanMode.Search:
                {

                    if (!string.IsNullOrWhiteSpace(Country) && Country.Length != 2)
                        throw new ArgumentOutOfRangeException(nameof(Country), Country, "Country must be a 2 letter country code.");

                    if (string.IsNullOrWhiteSpace(ApiKey))
                        throw new ArgumentNullException(nameof(ApiKey), "API Key must be specified.");

                    break;
                }
                case ScanMode.Import:
                {
                    if (!InputFile.Exists)
                        throw new FileNotFoundException("Input file does not exist or cannot be read.", InputFile.FullName);

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(ScanMode), ScanMode, "Invalid scan mode specified.");
            }

            if (LogFile == null)
                return;

            try
            {
                if (!LogFile.Directory.Exists)
                    LogFile.Directory.Create();
            }
            catch
            {
                throw new IOException("Could not create the output file directory. Check file system permissions.");
            }
        }
    }

    public enum ScanMode
    {
        Search,
        Import
    }
}
