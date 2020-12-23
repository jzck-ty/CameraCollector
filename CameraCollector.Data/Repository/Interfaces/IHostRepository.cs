using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CameraCollector.Core.Entities;

namespace CameraCollector.Data.Interfaces
{
    public interface IHostRepository
    {
        Task<List<Host>> GetHosts();

        Task<Host> GetHost(Guid hostId);

        Task<Host> GetHostByIpAddress(string ipAddress);

        Task AddHost(Host host);

        Task UpdateHost(Host host);

        Task DeleteHost(Guid hostId);
    }
}
