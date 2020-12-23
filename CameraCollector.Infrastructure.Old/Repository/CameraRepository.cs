using System;
using System.Threading.Tasks;
using CameraCollector.Core.Entities;
using CameraCollector.Core.Interfaces;
using CameraCollector.Data.Data;

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
            return await context.Cameras.FindAsync(cameraId);
        }

        public async void AddCamera(Camera camera)
        {
            await context.Cameras.AddAsync(camera);
            await context.SaveChangesAsync();
        }

        public void UpdateCamera(Camera camera)
        {
            context.Cameras.Update(camera);
            context.SaveChanges();
        }

        public void DeleteCamera(Guid cameraId)
        {
            var camera = context.Cameras.FindAsync(cameraId);
            context.Remove(camera);
            context.SaveChanges();
        }
    }
}
