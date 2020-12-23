using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CameraCollector.Core.Entities;
using CameraCollector.Core.Interfaces;
using CameraCollector.Data.Data;
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
            return await context.Hosts.ToListAsync();
        }

        public async Task<Host> GetHost(Guid hostId)
        {
            return await context.Hosts.FindAsync(hostId);
        }

        public async void AddHost(Host host)
        {
            await context.Hosts.AddAsync(host);
            await context.SaveChangesAsync();
        }

        public void UpdateHost(Host host)
        {
            context.Hosts.Update(host);
            context.SaveChanges();
        }

        public void DeleteHost(Guid hostId)
        {
            var host = context.Hosts.Find(hostId);
            context.Remove(host);
            context.SaveChanges();
        }
    }
}
