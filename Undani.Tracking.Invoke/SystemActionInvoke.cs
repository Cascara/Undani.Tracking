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
        private Guid _userId;
        private string _token;

        public SystemActionInvoke(IConfiguration configuration, Guid userId, string token)
        {
            _configuration = configuration;
            _userId = userId;
            _token = token;
        }

        public IConfiguration Configuration
        {
            get { return _configuration; }
        }

        public Guid UserId
        {
            get { return _userId; }
        }

        public string Token
        {
            get { return _token; }
        }

        public bool Invoke(Guid systemActionInstanceId, string method, string alias, string configuration)
        {
            MethodInfo methodInfo = typeof(SystemActionInvoke).GetMethod(method);

            if (methodInfo.IsStatic)
                return Convert.ToBoolean(methodInfo.Invoke(null, new object[] { systemActionInstanceId, alias, configuration }));
            else
                return Convert.ToBoolean(methodInfo.Invoke(this, new object[] { systemActionInstanceId, alias, configuration }));
        }
    }
}
