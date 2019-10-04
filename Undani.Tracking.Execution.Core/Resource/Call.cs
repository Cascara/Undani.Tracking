using Microsoft.Extensions.Configuration;

namespace Undani.Tracking.Execution.Core.Resource
{
    internal abstract class Call
    {
        private IConfiguration _configuration;

        public Call(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IConfiguration Configuration
        {
            get { return _configuration; }
        }
    }
}
