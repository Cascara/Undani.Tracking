using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Text;
using Undani.Tracking.Execution.Core.Invoke.Resource;
using Undani.Tracking.Execution.Core.Invoke.Infra;
using System.Data;
using Newtonsoft.Json;
using System.Dynamic;
using Newtonsoft.Json.Converters;
using Undani.Tracking.Execution.Core;

namespace Undani.Tracking.Core.Invoke
{
    public partial class SystemActionInvoke
    {
        public bool Integration(Guid systemActionInstanceId, string alias, string configuration)
        {
            bool start = false;
            switch (alias)
            {
                case "FormInstanceIntegration":
                    start = FormInstanceIntegration(systemActionInstanceId, configuration);
                    break;

                default:
                    throw new Exception("The method is not implemented");
            }

            return start;
        }

        private bool FormInstanceIntegration(Guid systemActionInstanceId, string configuration)
        {
            bool start = false;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_FormInstanceIntegration", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@FormInstanceId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    configuration = configuration.Replace("[FormInstanceId]", cmd.Parameters["@FormInstanceId"].Value.ToString());
                }
            }

            start = new IntegrationCall(Configuration).ExecuteFormInstanceIntegration(configuration);

            return start;
        }
    }
}
