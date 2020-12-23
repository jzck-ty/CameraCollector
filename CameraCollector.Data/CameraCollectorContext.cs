using System;
using System.Collections.Generic;
using CameraCollector.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CameraCollector.Data
{
    public class CameraCollectorContext : DbContext
    {
        private readonly IConfiguration configuration;

        public CameraCollectorContext(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public CameraCollectorContext(DbContextOptions<CameraCollectorContext> options, IConfiguration configuration) :
            base(options)
        {
            this.configuration = configuration;
        }

        public DbSet<Host> Hosts { get; set; }
        public DbSet<Camera> Cameras { get; set; }
        public DbSet<CameraType> CameraTypes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbConnectionString = configuration.GetConnectionString("CameraContext");
            optionsBuilder.UseSqlServer(dbConnectionString);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CameraType>().HasData(new List<CameraType>
            {
                new CameraType
                {
                    Id = Guid.NewGuid(),
                    Name = "foscam",
                    DefaultUsername = "admin",
                    DefaultPassword = "",
                    StreamUrl = "videostream.cgi",
                    SearchTerm = "netwave camera"
                }
            });
        }
    }
}
