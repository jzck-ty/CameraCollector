using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CameraCollector.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CameraCollector.ConsoleApp
{
    class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CameraCollectorContext>
    {
        public CameraCollectorContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<CameraCollectorContext>();
            var connectionString = configuration.GetConnectionString("CameraContext");
            builder.UseSqlServer(connectionString);
            return new CameraCollectorContext(builder.Options, configuration);
        }
    }
}
