using CameraCollector.Core;
using CameraCollector.Core.Entities;
using CameraCollector.Data.Interfaces;
using ConsoleProgressBar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CameraCollector.ConsoleApp
{
    public class ScanBroker
    {
        private readonly IHostRepository hostRepository;
        private readonly ICameraRepository cameraRepository;
        private readonly ICameraTypeRepository cameraTypeRepository;

        private ScannerConfiguration scannerConfiguration;

        private int iterations;
        private int totalCamerasWithAdditions;
        private DateTime scanStart;
        
        private ConcurrentQueue<Camera> scrapedCams;
        private readonly List<Camera> goodCams;
        private readonly List<int> foscamCommonPorts;
        private readonly BackgroundWorker threadManager;

        public ScanBroker(IHostRepository hostRepository, ICameraRepository cameraRepository, ICameraTypeRepository cameraTypeRepository)
        {
            scrapedCams = new ConcurrentQueue<Camera>();
            goodCams = new List<Camera>();

            this.hostRepository = hostRepository;
            this.cameraRepository = cameraRepository;
            this.cameraTypeRepository = cameraTypeRepository;

            threadManager = new BackgroundWorker { WorkerSupportsCancellation = true };
            threadManager.DoWork += DoWork;
            threadManager.RunWorkerCompleted += WorkComplete;

            foscamCommonPorts = new List<int> { 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 3000, 3001, 8008, 8080, 8081, 8090, 8091, 9090, 9091 };
        }

        public async Task Run(ScannerConfiguration scannerConfig)
        {
            scannerConfiguration = scannerConfig;

            switch (scannerConfiguration.ScanMode)
            {
                case ScanMode.Search:
                    await Search();

                    break;
                case ScanMode.Import:
                    await Import();

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task Search()
        {
            //Determine camera type
            var cameraType = await cameraTypeRepository.GetCameraTypeByName(scannerConfiguration.CameraTypeName);

            if (cameraType == null || cameraType.Id == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(cameraType), cameraType, "Not a valid camera type");

            var scanner = new CameraScanner(cameraType, scannerConfiguration.ApiKey, scannerConfiguration.Country, scannerConfiguration.Ports, scannerConfiguration.Pages);

            Console.WriteLine("Searching for cameras on Shodan.io. Please wait...");

            var searchResults = await scanner.Search();

            //Parse Camera Queue
            scrapedCams = scanner.Parse(searchResults, scannerConfiguration.OptimisticCrawl, scannerConfiguration.AdjacentPortDepth,
                scannerConfiguration.CrawlCommonPorts ? foscamCommonPorts : null);

            await RunScan();
        }

        public async Task Import()
        {
            //Determine camera type
            var cameraType = await cameraTypeRepository.GetCameraTypeByName(scannerConfiguration.CameraTypeName);

            if (cameraType == null || cameraType.Id == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(cameraType), cameraType, "Not a valid camera type");

            Console.WriteLine("Getting hosts from input file. Please wait...");

            var hosts = CameraScanner.GetImportedHosts(scannerConfiguration.InputFile, cameraType, scannerConfiguration.OptimisticCrawl, scannerConfiguration.AdjacentPortDepth,
                scannerConfiguration.CrawlCommonPorts ? foscamCommonPorts : null);

            scrapedCams = new ConcurrentQueue<Camera>();
            foreach (var camera in hosts.SelectMany(host => host.Cameras))
            {
                scrapedCams.Enqueue(camera);
            }

            await RunScan();
        }

        private async Task RunScan()
        {
            Console.Clear();

            scanStart = DateTime.UtcNow;
            threadManager.RunWorkerAsync();

            var totalCams = scrapedCams.Count;
            totalCamerasWithAdditions = totalCams;

            var consoleStringBuilder = new StringBuilder($"Scanning {scrapedCams.Count:###,###}");

            if (scannerConfiguration.AdjacentPortDepth > 0 || scannerConfiguration.CrawlCommonPorts)
                consoleStringBuilder.Append(" possible");

            consoleStringBuilder.Append(" cameras");

            if (scannerConfiguration.AdjacentPortDepth > 0 || scannerConfiguration.CrawlCommonPorts)
            {
                if (scannerConfiguration.AdjacentPortDepth > 0 && scannerConfiguration.CrawlCommonPorts)
                    consoleStringBuilder.Append(" with adjacent and common ports");
                else if (scannerConfiguration.AdjacentPortDepth > 0)
                    consoleStringBuilder.Append(" with adjacent ports");
                else
                    consoleStringBuilder.Append(" with common ports");

                consoleStringBuilder.Append(scannerConfiguration.OptimisticCrawl ? " (optimistic)" : string.Empty);
            }

            consoleStringBuilder.Append("...");

            Console.WriteLine(consoleStringBuilder.ToString());

            using (var progressBar = new ProgressBar(totalCams))
            {
                var remainingSlowUpdate = TimeSpan.Zero;
                var lastTimeSample = DateTime.UtcNow;

                while (threadManager.IsBusy)
                {
                    if (iterations > totalCams)
                        iterations = totalCams;

                    var verboseProgressStringBuilder = new StringBuilder($"- {goodCams.Count:###,##0} / {totalCamerasWithAdditions:###,###} open cameras");

                    if (iterations != 0 && totalCams != 0 && DateTime.UtcNow.Subtract(lastTimeSample).TotalMilliseconds > 1000)
                    {
                        var elapsed = DateTime.UtcNow.Subtract(scanStart);
                        var timeRemaining = (elapsed / iterations) * (totalCams - iterations);

                        lastTimeSample = DateTime.UtcNow;
                        remainingSlowUpdate = timeRemaining;
                    }

                    if (remainingSlowUpdate != TimeSpan.Zero)
                        verboseProgressStringBuilder.Append($" - {remainingSlowUpdate:d\\.hh\\:mm\\:ss}");

                    progressBar.Report(iterations, verboseProgressStringBuilder.ToString());
                    Thread.Sleep(20);
                }
            }

            if (scannerConfiguration.LogFile != null)
            {
                await using var streamWriter = new StreamWriter(scannerConfiguration.LogFile.FullName);

                streamWriter.WriteLine("IP");

                foreach (var cam in goodCams.OrderBy(c => c.Host.IpAddress).ThenBy(c => c.Port))
                {
                    streamWriter.WriteLine($"http://{cam.Host.IpAddress}:{cam.Port}");
                }
            }

            //Parse good cams to host list
            var hosts = ParseGoodCamsToHosts();

            //Save hosts and cams to database
            await SaveHostsToDatabase(hosts);

            Console.WriteLine("Done");
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            var threadCount = scannerConfiguration.Threads;
            var threadArray = new Thread[threadCount];

            for (var i = 0; i < threadArray.Length; i++)
            {
                var threadDelegate = new ThreadStart(GetAndScanNextCam);
                threadArray[i] = new Thread(threadDelegate);
                threadArray[i].Start();
            }

            foreach (var thread in threadArray)
            {
                thread.Join();
            }
        }

        private void WorkComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine($"Time taken - {DateTime.UtcNow.Subtract(scanStart):d\\.hh\\:mm\\:ss}");
        }

        private void GetAndScanNextCam()
        {
            while (scrapedCams.Count > 0)
            {
                if (!scrapedCams.TryDequeue(out var cam))
                    continue;

                if (CameraScanner.AttemptConnection(cam) && !goodCams.Any(c => c.Host.IpAddress == cam.Host.IpAddress && c.Port == cam.Port))
                {
                    goodCams.Add(cam);

                    if (scannerConfiguration.OptimisticCrawl)
                        continue;

                    var additionalCams = new List<Camera>();

                    if (scannerConfiguration.AdjacentPortDepth > 0)
                    {
                        var adjacentCams = new List<Camera>();

                        for (var i = 1; i <= scannerConfiguration.AdjacentPortDepth; i++)
                        {
                            adjacentCams.AddRange(new List<Camera>
                            {
                                CameraScanner.MapCamera(cam.Host, cam.Port - i, cam.CameraType),
                                CameraScanner.MapCamera(cam.Host, cam.Port + i, cam.CameraType)
                            });

                            totalCamerasWithAdditions += 2;
                        }

                        additionalCams.AddRange(adjacentCams);
                    }

                    if (scannerConfiguration.CrawlCommonPorts)
                    {
                        foreach (var port in foscamCommonPorts.Where(p => additionalCams.All(c => c.Port != p)))
                        {
                            additionalCams.Add(CameraScanner.MapCamera(cam.Host, port, cam.CameraType));

                            totalCamerasWithAdditions++;
                        }
                    }

                    foreach (var additionalCamera in additionalCams.Where(CameraScanner.AttemptConnection))
                    {
                        if (!goodCams.Any(c => c.Host.IpAddress == additionalCamera.Host.IpAddress && c.Port == additionalCamera.Port))
                            goodCams.Add(additionalCamera);
                    }
                }

                iterations++;
            }
        }

        private List<Host> ParseGoodCamsToHosts()
        {
            var hosts = new List<Host>();

            foreach (var cam in goodCams)
            {
                if (hosts.Count(h => h.IpAddress == cam.Host.IpAddress) == 0)
                {
                    var host = cam.Host;

                    if (host.Cameras.All(c => c.Port != cam.Port))
                        host.Cameras.Add(cam);

                    hosts.Add(host);
                }
                else
                {
                    var host = hosts.First(h => h.IpAddress == cam.Host.IpAddress);

                    if (host.Cameras.All(c => c.Port != cam.Port))
                        host.Cameras.Add(cam);
                }
            }

            return hosts;
        }

        public async Task SaveHostsToDatabase(List<Host> hosts)
        {
            foreach (var host in hosts)
            {
                var matchingHost = await hostRepository.GetHostByIpAddress(host.IpAddress);

                if (matchingHost == null || matchingHost.Id == Guid.Empty)
                {
                    try
                    {
                        await hostRepository.AddHost(host);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                else
                {
                    try
                    {
                        foreach (var hostCam in host.Cameras)
                        {
                            if (matchingHost.Cameras.Any(c => c.Port == hostCam.Port))
                                continue;

                            hostCam.Host = matchingHost;
                            matchingHost.Cameras.Add(hostCam);

                            await cameraRepository.AddCamera(hostCam);
                        }

                        matchingHost.LastPinged = host.LastPinged;

                        await hostRepository.UpdateHost(host);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine($"Host: {host.IpAddress}. Cameras:");
                        foreach(var c in host.Cameras)
                        {
                            Console.WriteLine(c.Port);
                        }
                    }
                }
            }
        }
    }
}
