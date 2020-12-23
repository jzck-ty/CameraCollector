using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CameraCollector.Core.Entities;

namespace CameraCollector.Data.Interfaces
{
    public interface ICameraRepository
    {
        Task<Camera> GetCamera(Guid cameraId);

        Task<Camera> GetCameraByHostIdAndPort(Guid hostId, int port);

        Task AddCamera(Camera camera);

        Task UpdateCamera(Camera camera);

        Task DeleteCamera(Guid cameraId);
    }
}
