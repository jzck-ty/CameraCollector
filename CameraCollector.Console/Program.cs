using CameraCollector.Data.Interfaces;
using CameraCollector.Data;
using CameraCollector.Data.Repository;
using CommandLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Hosting;

namespace CameraCollector.ConsoleApp
{
    internal class Program
    {
        public static IConfiguration Configuration { get; private set; }

        private static void Main(string[] args)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddUserSecrets<Program>()
                .Build();
            
            var scannerConfiguration = new ScannerConfiguration(args);

            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient<ScanBroker>()
                        .AddSingleton(_ => Configuration)
                        .AddScoped<IHostRepository, HostRepository>()
                        .AddScoped<ICameraRepository, CameraRepository>()
                        .AddScoped<ICameraTypeRepository, CameraTypeRepository>()
                        .AddDbContext<CameraCollectorContext>(options => options.UseSqlServer(Configuration.GetConnectionString("CameraContext")));
                })
                .UseConsoleLifetime();

            var host = builder.Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;

                try
                {
                    var scanBroker = services.GetRequiredService<ScanBroker>();
                    scanBroker.Run(scannerConfiguration).Wait();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public static void ReportError(IEnumerable<Error> obj)
        {
            Console.WriteLine("Some shite went wrong with your parameters there, bud.");
            
            Environment.Exit(-1);
        }
    }
}
