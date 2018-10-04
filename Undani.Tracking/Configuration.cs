using Microsoft.Extensions.Configuration;
using System.IO;

namespace Undani.Tracking.Execution
{
    public static class Configuration
    {
        private static IConfiguration _configuration;

        static Configuration()
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

            _configuration = builder.Build();
        }

        public static string GetValue(string key)
        {
            return _configuration[key];
        }
    }
}
