using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CameraCollector.Core.Entities;
using CameraCollector.Core.Interfaces;
using CameraCollector.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace CameraCollector.Data.Repository
{
    public class CameraTypeRepository : ICameraTypeRepository
    {
        private readonly CameraCollectorContext context;

        public CameraTypeRepository(CameraCollectorContext context)
        {
            this.context = context;
        }

        public async Task<List<CameraType>> GetCameraTypes()
        {
            return await context.CameraTypes.ToListAsync();
        }

        public async Task<CameraType> GetCameraType(Guid cameraTypeId)
        {
            return await context.CameraTypes.FindAsync(cameraTypeId);
        }

        public async void AddCameraType(CameraType cameraType)
        {
            await context.CameraTypes.AddAsync(cameraType);
            await context.SaveChangesAsync();
        }

        public void UpdateCameraType(CameraType cameraType)
        {
            context.CameraTypes.Update(cameraType);
            context.SaveChanges();
        }

        public void DeleteCameraType(Guid cameraTypeId)
        {
            var cameraType = context.CameraTypes.Find(cameraTypeId);
            context.CameraTypes.Remove(cameraType);
            context.SaveChanges();
        }
    }
}
