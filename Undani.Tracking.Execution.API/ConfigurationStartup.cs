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
                var dict = Undani.Configuration.Load(
                    Environment.GetEnvironmentVariable("CONFIGURATION_ENVIRONMENT"),
                    Environment.GetEnvironmentVariable("CONFIGURATION_SYSTEM")
                    );

                config.AddInMemoryCollection(dict);
            });
        }
    }
}
