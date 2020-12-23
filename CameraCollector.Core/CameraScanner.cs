using CameraCollector.Core.Entities;
using Shodan.Net.Models;
using Shodan.Net.Standard;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Host = CameraCollector.Core.Entities.Host;
using System.ComponentModel.Design;

namespace CameraCollector.Core
{
    public class CameraScanner
    {
        //private static readonly List<int> SearchPorts = new List<int> { 80, 81, 82, 83, 84, 85, 88, 3000, 8090 };
        private readonly ShodanClient client;
        private readonly CameraType cameraType;
        private readonly List<int> searchPorts;
        private readonly int pages;
        private readonly string countryCode;

        public CameraScanner(CameraType cameraType, string shodanApiKey, string countryCode = null, List<int> searchPorts = null, int pages = 1)
        {
            client = new ShodanClient(shodanApiKey);
            this.cameraType = cameraType;
            //this.searchPorts = searchPorts ?? new List<int>
            //    {
            //        80,
            //        81,
            //        82,
            //        83,
            //        84,
            //        85,
            //        88,
            //        3000,
            //        8090
            //    };
            this.searchPorts = searchPorts;
            this.pages = pages;
            this.countryCode = countryCode;
        }

        public async Task<List<Host>> Search()
        {
            var masterList = new List<Banner>();
            var hostList = new List<Host>();

            if (searchPorts != null && searchPorts.Count > 0)
            {
                foreach (var port in searchPorts)
                {
                    var page = 1;
                    do
                    {
                        try
                        {
                            var results = await client.SearchHosts(q =>
                                {
                                    q.SearchTerm(cameraType.SearchTerm.ToLowerInvariant())
                                        .WithPort(port);

                                    if (!string.IsNullOrWhiteSpace(countryCode))
                                        q.WithCountry(countryCode);
                                },
                                page: page);

                            masterList.AddRange(results.Matches.ToList());
                        }
                        catch
                        {
                            break;
                        }
                        page++;
                    } while (page <= pages);
                }
            }
            else
            {
                var page = 1;
                do
                {
                    try
                    {
                        var results = await client.SearchHosts(q =>
                        {
                            q.SearchTerm(cameraType.SearchTerm.ToLowerInvariant());

                            if (!string.IsNullOrWhiteSpace(countryCode))
                                q.WithCountry(countryCode);
                        },
                        page: page);

                        masterList.AddRange(results.Matches.ToList());
                    }
                    catch (Exception e)
                    {
                        Console.Write(e.Message);
                        break;
                    }
                    page++;
                } while (page <= pages);
            }

            // Parse Shodan Banner results to Host List
            foreach (var result in masterList)
            {
                if (hostList.Count(h => h.IpAddress == result.IpStr) == 0)
                {
                    var host = MapHost(result);
                    var camera = MapCamera(host, result.Port, cameraType);
                    host.Cameras.Add(camera);
                    hostList.Add(host);
                }
                else
                {
                    var host = hostList.First(h => h.IpAddress == result.IpStr);
                    var camera = MapCamera(host, result.Port, cameraType);

                    if (host.Cameras.Count(c => c.Port == camera.Port) == 0)
                    {
                        host.Cameras.Add(camera);
                    }
                }
            }

            return hostList;
        }

        public ConcurrentQueue<Camera> Parse(List<Host> hosts, bool optimistic, int adjacentDepth = 0, List<int> commonPorts = null)
        {
            var cameraQueue = new ConcurrentQueue<Camera>();

            // get all of the hosts' cameras and queue them up for scanning as well as process adjacent/common ports per config
            foreach (var cam in hosts.SelectMany(h => h.Cameras.OrderBy(c => c.Port)))
            {
                if (!cameraQueue.Any(c => c.Host.IpAddress == cam.Host.IpAddress && c.Port == cam.Port))
                    cameraQueue.Enqueue(cam);

                if (!optimistic)
                    continue;

                if (adjacentDepth > 0)
                {
                    var adjacentCams = new List<Camera>();

                    for (var i = 1; i <= adjacentDepth; i++)
                    {
                        adjacentCams.AddRange(new List<Camera>
                        {
                            MapCamera(cam.Host, cam.Port - i, cam.CameraType),
                            MapCamera(cam.Host, cam.Port + i, cam.CameraType)
                        });
                    }

                    foreach (var adjacentCam in adjacentCams.Where(aC => !cameraQueue.Any(c => c.Host.IpAddress == aC.Host.IpAddress && c.Port == aC.Port)))
                    {
                        cameraQueue.Enqueue(adjacentCam);
                    }
                }

                if (commonPorts == null)
                    continue;

                foreach (var port in commonPorts.Where(p => !cameraQueue.Any(c => c.Host.IpAddress == cam.Host.IpAddress && c.Port == p)))
                {
                    cameraQueue.Enqueue(MapCamera(cam.Host, port, cam.CameraType));
                }
            }

            return cameraQueue;
        }

        public static List<Host> GetImportedHosts(FileInfo inputFile, CameraType cameraType, bool optimisticCrawl = true, int adjacentPortDepth = 0, List<int> commonPorts = null)
        {
            var hosts = new List<Host>();

            using (var sR = new StreamReader(inputFile.FullName))
            {
                while (sR.Peek() >= 0)
                {
                    var json = sR.ReadLine();
                    var jObject = (JObject)JsonConvert.DeserializeObject(json);

                    Host scrapedHost;
                    var hostAlreadyInList = false;

                    if (hosts.Any(h => h.IpAddress == (string)jObject["ip_str"]))
                    {
                        scrapedHost = hosts.First(h => h.IpAddress == (string)jObject["ip_str"]);
                        hostAlreadyInList = true;
                    }
                    else
                    {
                        scrapedHost = new Host
                        {
                            Id = Guid.NewGuid(),
                            Active = true,
                            Cameras = new List<Camera>(),
                            City = (string)jObject["location"]["city"],
                            Country = (string)jObject["location"]["country_code"],
                            FoundOn = DateTime.UtcNow,
                            LastPinged = DateTime.UtcNow,
                            IpAddress = (string)jObject["ip_str"]
                        };
                    }

                    var scrapedCamera = new Camera
                    {
                        CameraType = cameraType,
                        Port = (int)jObject["port"],
                        Active = true,
                        FoundOn = DateTime.UtcNow,
                        LastPinged = DateTime.UtcNow,
                        Id = Guid.NewGuid(),
                        Host = scrapedHost
                    };

                    if (scrapedHost.Cameras.All(c => c.Port != scrapedCamera.Port))
                        scrapedHost.Cameras.Add(scrapedCamera);

                    if (!optimisticCrawl)
                    {
                        if (!hostAlreadyInList)
                            hosts.Add(scrapedHost);
                        continue;
                    }

                    if (adjacentPortDepth > 0)
                    {
                        var adjacentCams = new List<Camera>();

                        for (var i = 1; i <= adjacentPortDepth; i++)
                        {
                            adjacentCams.AddRange(new List<Camera>
                            {
                                new Camera
                                {
                                    Id = Guid.NewGuid(),
                                    CameraType = cameraType,
                                    Active = true,
                                    FoundOn = DateTime.UtcNow,
                                    LastPinged = DateTime.UtcNow,
                                    Port = scrapedCamera.Port - i,
                                    Host = scrapedHost
                                },
                                new Camera
                                {
                                    Id = Guid.NewGuid(),
                                    CameraType = cameraType,
                                    Active = true,
                                    FoundOn = DateTime.UtcNow,
                                    LastPinged = DateTime.UtcNow,
                                    Port = scrapedCamera.Port + i,
                                    Host = scrapedHost
                                }
                            });
                        }

                        foreach (var adjacentCamera in adjacentCams.Where(aC => scrapedHost.Cameras.All(c => c.Port != aC.Port)))
                        {
                            scrapedHost.Cameras.Add(adjacentCamera);
                        }
                    }

                    if (commonPorts == null)
                    {
                        if (!hostAlreadyInList)
                            hosts.Add(scrapedHost);
                        continue;
                    }

                    foreach (var port in commonPorts.Where(port => scrapedHost.Cameras.All(c => c.Port != port)))
                    {
                        scrapedHost.Cameras.Add(new Camera
                        {
                            Id = Guid.NewGuid(),
                            CameraType = cameraType,
                            Active = true,
                            FoundOn = DateTime.UtcNow,
                            LastPinged = DateTime.UtcNow,
                            Port = port,
                            Host = scrapedHost
                        });
                    }

                    if (!hostAlreadyInList)
                        hosts.Add(scrapedHost);
                }
            }

            return hosts;
        }

        public static bool AttemptConnection(Camera camera)
        {
            // sometimes this happens when you're using a bunch of threads. it's not a problem.
            if (camera == null)
                return false;

            // Attempt to connect to the server up to three times with timeouts 2.5s, 5s, & 10s
            bool mainRetry;
            var mainRetries = 0;
            do
            {
                mainRetry = false;

                try
                {
                    var req = WebRequest.Create($"http://{camera.Host.IpAddress}:{camera.Port}/{camera.CameraType.StreamUrl}");
                    req.Credentials = new NetworkCredential(camera.UserName, camera.Password);
                    req.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.BypassCache);    //Bypass cache so we don't cache the timeout
                    req.Timeout = 2500 * (2 ^ mainRetries);

                    var resp = (HttpWebResponse)req.GetResponse();

                    if (resp.StatusCode != HttpStatusCode.OK)
                        return false;

                    // make sure that the server is telling us it's a camera
                    // sometimes this fails because shitty servers don't have a server header
                    if (camera.CameraType.Name == "foscam")
                    {
                        try
                        {
                            if (!resp.Headers["Server"].ToLowerInvariant().Contains("netwave"))
                                return false;
                        }
                        catch
                        {
                            return false;
                        }
                    }

                    //check known bad url for fake cams.
                    //retry up to 3 times, same as main loop
                    bool validationRetry;
                    var validationRetries = 0;
                    do
                    {
                        validationRetry = false;

                        // we want to hit the WebException - hopefully the server's throwing a 404
                        try
                        {
                            var validationRequest = WebRequest.Create($"http://{camera.Host.IpAddress}:{camera.Port}/shitassbitchtits.html");
                            validationRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.BypassCache);
                            validationRequest.Timeout = 2500 * (2 ^ validationRetries);

                            var resp2 = (HttpWebResponse)validationRequest.GetResponse();

                            // why is a 404 handled as an exception (it throws one) if we can handle it here?
                            switch (resp2.StatusCode)
                            {
                                case HttpStatusCode.OK:
                                    return false;
                                case HttpStatusCode.NotFound:
                                    return true;
                            }
                        }
                        catch (WebException ex)
                        {
                            var errorResp = ex.Response as HttpWebResponse;

                            // if it's a 404, we know it's a good camera
                            if (errorResp?.StatusCode == HttpStatusCode.NotFound)
                                return true;

                            // any other error response = retry
                            validationRetry = true;
                            validationRetries++;
                        }
                        catch
                        {
                            // something else happened and we should just ignore this camera
                            return false;
                        }
                    } while (validationRetry && validationRetries < 4);

                    // out of retries
                    return false;
                }
                catch (WebException ex)
                {
                    var errorResp = ex.Response as HttpWebResponse;

                    // if we have a 401 or 403 then we aren't getting in with default creds, which is all we're interested in
                    if (errorResp?.StatusCode == HttpStatusCode.Forbidden || errorResp?.StatusCode == HttpStatusCode.Unauthorized)
                        return false;

                    // any other error response = retry
                    mainRetry = true;
                    mainRetries++;
                }
                catch
                {
                    // something else happened and we should just ignore this camera
                    return false;
                }
            } while (mainRetry && mainRetries < 4);

            // out of retries
            return false;
        }

        private static Host MapHost(Banner result)
        {
            return new Host
            {
                Id = Guid.NewGuid(),
                Cameras = new List<Camera>(),
                Active = true,
                City = result.Location.City,
                Country = result.Location.CountryCode,
                FoundOn = DateTime.UtcNow,
                LastPinged = DateTime.UtcNow,
                IpAddress = result.IpStr
            };
        }

        public static Camera MapCamera(Host host, int port, CameraType camType)
        {
            return new Camera
            {
                Id = Guid.NewGuid(),
                Active = true,
                CameraTypeId = camType.Id,
                CameraType = camType,
                FoundOn = DateTime.UtcNow,
                LastPinged = DateTime.UtcNow,
                Port = port,
                UserName = camType.DefaultUsername,
                Password = camType.DefaultPassword,
                HostId = host.Id,
                Host = host
            };
        }
    }
}
