using CameraCollector.Core.Entities;
using CameraCollector.Data;
using CameraCollector.Data.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CameraCollector.Test
{
    [TestClass]
    public class DataTests
    {
        private HostRepository hostRepository;
        private CameraRepository cameraRepository;
        private CameraTypeRepository cameraTypeRepository;
        private CameraCollectorContext context;

        public DataTests()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            context = new CameraCollectorContext(configuration);

            hostRepository = new HostRepository(context);
            cameraRepository = new CameraRepository(context);
            cameraTypeRepository = new CameraTypeRepository(context);
        }

        #region Create

        [TestMethod]
        public async Task SaveNewHost()
        {
            var hostIn = await CreateHost();

            await hostRepository.AddHost(hostIn);

            var host = await hostRepository.GetHost(hostIn.Id);

            Assert.IsNotNull(host);
            Assert.AreEqual(hostIn.Id, host.Id);
        }

        [TestMethod]
        public async Task SaveNewCamera()
        {
            var hostIn = await GetSingleHost();
            var camera = await CreateCamera(hostIn);

            await cameraRepository.AddCamera(camera);

            var cameraOut = await cameraRepository.GetCamera(camera.Id);

            var hostOut = await hostRepository.GetHost(hostIn.Id);

            Assert.IsNotNull(cameraOut);
            Assert.AreEqual(camera.Id, cameraOut.Id);
        }

        [TestMethod]
        public async Task SaveNewCameraType()
        {
            var cameraTypeIn = CreateCameraType();

            await cameraTypeRepository.AddCameraType(cameraTypeIn);

            var cameraType = await cameraTypeRepository.GetCameraType(cameraTypeIn.Id);

            Assert.IsNotNull(cameraType);
            Assert.AreEqual(cameraTypeIn.Id, cameraType.Id);
            Assert.AreEqual(cameraTypeIn.Name, cameraType.Name);
        }

        #endregion

        #region Read

        #region Hosts

        [TestMethod]
        public async Task GetAllHosts()
        {
            var hosts = await hostRepository.GetHosts();

            Assert.IsNotNull(hosts);
            Assert.IsTrue(hosts.Count > 0);
        }

        [TestMethod]
        public async Task GetSpecificHostById()
        {
            var hostIn = await GetSingleHost();

            var host = await hostRepository.GetHost(hostIn.Id);

            Assert.IsNotNull(host);
            Assert.AreEqual(hostIn.Id, host.Id);
        }

        [TestMethod]
        public async Task GetSpecificHostByIP()
        {
            var hostIn = await GetSingleHost();

            var host = await hostRepository.GetHostByIpAddress(hostIn.IpAddress);

            Assert.IsNotNull(host);
            Assert.AreEqual(hostIn.Id, host.Id);
            Assert.AreEqual(hostIn.IpAddress, host.IpAddress);
        }

        #endregion

        #region Cameras

        [TestMethod]
        public async Task GetCameraById()
        {
            var cameraIn = await GetSingleCamera();
            var camera = await cameraRepository.GetCamera(cameraIn.Id);

            Assert.IsNotNull(camera);
            Assert.AreEqual(cameraIn.Id, camera.Id);
        }

        [TestMethod]
        public async Task GetCameraByHostIdAndPort()
        {
            var cameraIn = await GetSingleCamera();
            var camera = await cameraRepository.GetCameraByHostIdAndPort(cameraIn.HostId, cameraIn.Port);

            Assert.IsNotNull(camera);
            Assert.AreEqual(cameraIn.Id, camera.Id);
            Assert.AreEqual(cameraIn.Port, camera.Port);
        }

        #endregion

        #region Camera Types

        [TestMethod]
        public async Task GetAllCameraTypes()
        {
            var cameraTypes = await cameraTypeRepository.GetCameraTypes();

            Assert.IsNotNull(cameraTypes);
            Assert.IsTrue(cameraTypes.Count > 0);
        }

        [TestMethod]
        public async Task GetSpecificCameraTypeById()
        {
            var cameraTypeIn = await GetSingleCameraType();
            var cameraType = await cameraTypeRepository.GetCameraType(cameraTypeIn.Id);

            Assert.IsNotNull(cameraType);
            Assert.AreEqual(cameraTypeIn.Id, cameraType.Id);
        }

        [TestMethod]
        public async Task GetSpecificCameraTypeByname()
        {
            var cameraTypeIn = await GetSingleCameraType();
            var cameraType = await cameraTypeRepository.GetCameraTypeByName(cameraTypeIn.Name);

            Assert.IsNotNull(cameraType);
            Assert.AreEqual(cameraTypeIn.Id, cameraType.Id);
            Assert.AreEqual(cameraTypeIn.Name, cameraType.Name);
        }

        #endregion

        #endregion

        #region Update

        [TestMethod]
        public async Task UpdateSingleHost()
        {
            var hostIn = await GetSingleHost();
            var cameraIn = await CreateCamera(hostIn);
            hostIn.LastPinged = DateTime.UtcNow;
            hostIn.Cameras.Add(cameraIn);

            await cameraRepository.AddCamera(cameraIn);
            await hostRepository.UpdateHost(hostIn);

            var hostOut = await hostRepository.GetHost(hostIn.Id);

            Assert.IsNotNull(hostOut);
            Assert.AreEqual(hostIn.Id, hostOut.Id);
            Assert.AreEqual(hostIn.LastPinged, hostOut.LastPinged);
            Assert.AreEqual(hostIn.Cameras.Count, hostOut.Cameras.Count);
        }

        [TestMethod]
        public async Task UpdateSingleCamera()
        {
            var cameraIn = await GetSingleCamera();
            cameraIn.LastPinged = DateTime.UtcNow;
            cameraIn.Port += new Random().Next(-20, 1000);

            await cameraRepository.UpdateCamera(cameraIn);

            var cameraOut = await cameraRepository.GetCamera(cameraIn.Id);

            Assert.IsNotNull(cameraOut);
            Assert.AreEqual(cameraIn.Id, cameraOut.Id);
            Assert.AreEqual(cameraIn.LastPinged, cameraOut.LastPinged);
            Assert.AreEqual(cameraIn.Port, cameraOut.Port);
        }

        [TestMethod]
        public async Task UpdateSingleCameraType()
        {
            var cameraTypeIn = await GetSingleCameraType();
            cameraTypeIn.SearchTerm = DateTime.UtcNow.ToString();

            await cameraTypeRepository.UpdateCameraType(cameraTypeIn);

            var cameraTypeOut = await cameraTypeRepository.GetCameraType(cameraTypeIn.Id);

            Assert.IsNotNull(cameraTypeOut);
            Assert.AreEqual(cameraTypeIn.Id, cameraTypeOut.Id);
            Assert.AreEqual(cameraTypeIn.SearchTerm, cameraTypeOut.SearchTerm);
        }

        #endregion

        #region Delete

        [TestMethod]
        public async Task DeleteHost()
        {
            var host = await GetSingleHost();
            await hostRepository.DeleteHost(host.Id);

            var hostOut = await hostRepository.GetHost(host.Id);

            Assert.IsNull(hostOut);
        }

        [TestMethod]
        public async Task DeleteCamera()
        {
            var camera = await GetSingleCamera();
            await cameraRepository.DeleteCamera(camera.Id);

            var cameraOut = await cameraRepository.GetCamera(camera.Id);

            Assert.IsNull(cameraOut);
        }

        [TestMethod]
        public async Task DeleteCameraType()
        {
            var cameraType = await GetSingleCameraType();
            await cameraTypeRepository.DeleteCameraType(cameraType.Id);

            var cameraTypeOut = await cameraTypeRepository.GetCameraType(cameraType.Id);

            Assert.IsNull(cameraTypeOut);
        }

        #endregion

        #region Helpers

        public async Task<Host> GetSingleHost()
        {
            var hosts = await hostRepository.GetHosts();

            var randIndex = new Random().Next(0, hosts.Count - 1);

            return hosts[randIndex];
        }

        public async Task<List<Host>> CreateHosts()
        {
            var hostCount = new Random().Next(1, 3);

            var hosts = new List<Host>();

            for (var i = 1; i < hostCount; i++)
            {
                hosts.Add(await CreateHost());
            }

            return hosts;
        }

        public async Task<Host> CreateHost()
        {
            var id = Guid.NewGuid();

            var host = new Host
            {
                Id = id,
                Active = true,
                City = "Testville",
                Country = "TS",
                IpAddress = "127.0.0.1",
                FoundOn = DateTime.UtcNow,
                LastPinged = DateTime.UtcNow,
                Name = "Testing Host",
            };

            host.Cameras = await CreateCameras(host);

            return host;
        }

        public async Task<Camera> GetSingleCamera()
        {
            Host host;
            do
            {
                host = await GetSingleHost();
            }
            while (host.Cameras.Count == 0);

            return host.Cameras.First();
        }

        public async Task<List<Camera>> CreateCameras(Host host)
        {
            var cameraCount = new Random().Next(1, 3);

            var cameras = new List<Camera>();

            for (var i = 1; i < cameraCount; i++)
            {
                cameras.Add(await CreateCamera(host, i));
            }

            return cameras;
        }

        public async Task<Camera> CreateCamera(Host host, int cameraIndex = 0)
        {
            var cameraType = await GetSingleCameraType();

            return new Camera
            {
                Id = Guid.NewGuid(),
                Active = true,
                CameraType = cameraType,
                CameraTypeId = cameraType.Id,
                HostId = host.Id,
                Host = host,
                Name = "Test Camera",
                Description = string.Empty,
                FoundOn = DateTime.UtcNow,
                LastPinged = DateTime.UtcNow,
                Port = 80 + cameraIndex,
                UserName = cameraType.DefaultUsername,
                Password = cameraType.DefaultPassword
            };
        }

        public async Task<CameraType> GetSingleCameraType()
        {
            var cameraTypes = await cameraTypeRepository.GetCameraTypes();

            var randIndex = new Random().Next(0, cameraTypes.Count - 1);

            return cameraTypes[randIndex];
        }

        public CameraType CreateCameraType()
        {
            return new CameraType
            {
                Id = Guid.NewGuid(),
                DefaultUsername = "admin",
                DefaultPassword = string.Empty,
                Name = new Random().Next(0, 5000) + "Test Camera Type" + new Random().Next(0, 5000),
                SearchTerm = "Test Camera Type",
                StreamUrl = "video.test"
            };
        }

        #endregion
    }
}
