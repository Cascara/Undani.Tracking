using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Core.Invoke.Resource
{
    public abstract class Call
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
