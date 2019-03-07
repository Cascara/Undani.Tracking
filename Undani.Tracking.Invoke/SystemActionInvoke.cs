using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace Undani.Tracking.Invoke
{
    public partial class SystemActionInvoke
    {
        private IConfiguration _configuration;

        public SystemActionInvoke(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IConfiguration Configuration
        {
            get { return _configuration; }
        }

        public bool Invoke(Guid systemActionInstanceId, string method, string alias, string configuration)
        {
            MethodInfo methodInfo = typeof(SystemActionInvoke).GetMethod(method);

            return Convert.ToBoolean(methodInfo.Invoke(null, new object[] { systemActionInstanceId, alias, configuration }));      
        }
    }
}
