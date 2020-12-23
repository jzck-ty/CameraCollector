using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CameraCollector.Core.Entities;
using CameraCollector.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CameraCollector.Data.Repository
{
    public class HostRepository : IHostRepository
    {
        private readonly CameraCollectorContext context;

        public HostRepository(CameraCollectorContext context)
        {
            this.context = context;
        }

        public async Task<List<Host>> GetHosts()
        {
            return await context.Hosts
                .Include(h => h.Cameras)
                .ThenInclude(c => c.CameraType)
                .ToListAsync();
        }

        public async Task<List<Host>> GetHostsWithCameras()
        {
            return await context.Hosts
                .Include(h => h.Cameras)
                .ThenInclude(c => c.CameraType)
                .Where(h => h.Cameras.Count > 0)
                .ToListAsync();
        }

        public async Task<Host> GetHost(Guid hostId)
        {
            return await context.Hosts
                .Include(h => h.Cameras)
                .ThenInclude(c => c.CameraType)
                .FirstOrDefaultAsync(h => h.Id == hostId);
        }

        public async Task<Host> GetHostByIpAddress(string ipAddress)
        {
            return await context.Hosts
                .Include(h => h.Cameras)
                .ThenInclude(c => c.CameraType)
                .FirstOrDefaultAsync(h => h.IpAddress == ipAddress);
        }

        public async Task AddHost(Host host)
        {
            await context.Hosts.AddAsync(host);
            await context.SaveChangesAsync();
        }

        public async Task UpdateHost(Host host)
        {
            context.Hosts.Update(host);
            await context.SaveChangesAsync();
        }

        public async Task DeleteHost(Guid hostId)
        {
            var host = await context.Hosts.FindAsync(hostId);
            context.Remove(host);
            await context.SaveChangesAsync();
        }
    }
}
