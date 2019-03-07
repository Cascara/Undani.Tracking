using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

[assembly: HostingStartup(typeof(Undani.Tracking.Execution.API.ConfigurationStartup))]
namespace Undani.Tracking.Execution.API
{
    internal class ConfigurationStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config =>
            {
                var dict = Undani.Configuration.Load("5F14190F-3493-4A18-AB6E-22F3639263C2", "BA1D83FD-3334-4D5E-B696-6D1D45F56CE9");

                config.AddInMemoryCollection(dict);
            });
        }
    }
}
