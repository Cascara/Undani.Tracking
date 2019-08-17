using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace Undani.Tracking.Core.Invoke
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
            bool result = false;

            switch (method)
            {
                case "Custom":
                    result = Custom(systemActionInstanceId, alias, configuration);
                    break;

                case "Identity":
                    result = Identity(systemActionInstanceId, alias, configuration);
                    break;

                case "Integration":
                    result = Integration(systemActionInstanceId, alias, configuration);
                    break;

                case "KeyCalculation":
                    result = KeyCalculation(systemActionInstanceId, alias, configuration);
                    break;

                case "State":
                    result = State(systemActionInstanceId, alias, configuration);
                    break;

                case "Template":
                    result = Template(systemActionInstanceId, alias, configuration);
                    break;

                case "Tracking":
                    result = Tracking(systemActionInstanceId, alias, configuration);
                    break;

                case "UserRole":
                    result = UserRole(systemActionInstanceId, alias, configuration);
                    break;
            }

            return result;
        }
    }
}
