using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CameraCollector.Core.Entities;
using CameraCollector.Data.Interfaces;
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

        public async Task<CameraType> GetCameraTypeByName(string cameraTypeName)
        {
            return await context.CameraTypes.FirstOrDefaultAsync(cT => cT.Name.Equals(cameraTypeName));
        }

        public async Task AddCameraType(CameraType cameraType)
        {
            await context.CameraTypes.AddAsync(cameraType);
            await context.SaveChangesAsync();
        }

        public async Task UpdateCameraType(CameraType cameraType)
        {
            context.CameraTypes.Update(cameraType);
            await context.SaveChangesAsync();
        }

        public async Task DeleteCameraType(Guid cameraTypeId)
        {
            var cameraType = await context.CameraTypes.FindAsync(cameraTypeId);
            context.CameraTypes.Remove(cameraType);
            await context.SaveChangesAsync();
        }
    }
}
