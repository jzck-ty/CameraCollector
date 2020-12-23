using System;
using System.Threading.Tasks;
using CameraCollector.Core.Entities;
using CameraCollector.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CameraCollector.Data.Repository
{
    public class CameraRepository : ICameraRepository
    {
        private readonly CameraCollectorContext context;

        public CameraRepository(CameraCollectorContext context)
        {
            this.context = context;
        }

        public async Task<Camera> GetCamera(Guid cameraId)
        {
            return await context.Cameras
                .Include(c => c.Host)
                .Include(c => c.CameraType)
                .FirstOrDefaultAsync(c => c.Id == cameraId);
        }

        public async Task<Camera> GetCameraByHostIdAndPort(Guid hostId, int port)
        {
            return await context.Cameras.FirstOrDefaultAsync(c => c.HostId == hostId && c.Port == port);
        }

        public async Task AddCamera(Camera camera)
        {
            await context.Cameras.AddAsync(camera);
            await context.SaveChangesAsync();
        }

        public async Task UpdateCamera(Camera camera)
        {
            context.Cameras.Update(camera);
            await context.SaveChangesAsync();
        }

        public async Task DeleteCamera(Guid cameraId)
        {
            var camera = await context.Cameras.FindAsync(cameraId);
            context.Remove(camera);
            await context.SaveChangesAsync();
        }
    }
}
