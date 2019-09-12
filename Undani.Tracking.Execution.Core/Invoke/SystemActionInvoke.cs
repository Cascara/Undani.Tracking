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

        public bool Invoke(Guid systemActionInstanceId, string method, string alias, string settings)
        {
            bool result = false;

            switch (method)
            {
                case "Custom":
                    result = Custom(systemActionInstanceId, alias, settings);
                    break;

                case "Identity":
                    result = Identity(systemActionInstanceId, alias, settings);
                    break;

                case "Integration":
                    result = Integration(systemActionInstanceId, alias, settings);
                    break;

                case "KeyCalculation":
                    result = KeyCalculation(systemActionInstanceId, alias, settings);
                    break;

                case "State":
                    result = State(systemActionInstanceId, alias, settings);
                    break;

                case "Template":
                    result = Template(systemActionInstanceId, alias, settings);
                    break;

                case "Tracking":
                    result = Tracking(systemActionInstanceId, alias, settings);
                    break;

                case "UserRole":
                    result = UserRole(systemActionInstanceId, alias, settings);
                    break;
            }

            return result;
        }

        private void SetConfiguration(Guid systemActionInstanceId, string settings)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SystemActionInstanceSettings", cn))
                {

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@Settings", SqlDbType.VarChar, -1) { Value = settings });

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
