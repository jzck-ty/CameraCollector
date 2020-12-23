using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CameraCollector.Core.Entities;

namespace CameraCollector.Data.Interfaces
{
    public interface ICameraTypeRepository
    {
        Task<List<CameraType>> GetCameraTypes();

        Task<CameraType> GetCameraType(Guid cameraTypeId);

        Task<CameraType> GetCameraTypeByName(string cameraTypeName);

        Task AddCameraType(CameraType cameraType);

        Task UpdateCameraType(CameraType cameraType);

        Task DeleteCameraType(Guid cameraTypeId);
    }
}
