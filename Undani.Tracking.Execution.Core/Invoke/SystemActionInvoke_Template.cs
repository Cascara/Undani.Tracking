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
        public bool Template(Guid systemActionInstanceId, string alias, string settings)
        {
            bool start = false;
            switch (alias)
            {
                case "FormInstanceRequest":
                    start = FormInstanceRequest(systemActionInstanceId, settings);
                    break;

                default:
                    throw new NotImplementedException();
            }

            return start;
        }

        private bool FormInstanceRequest(Guid systemActionInstanceId, string settings)
        {
            bool start = false;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_FormInstanceRequest", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@EnvironmentId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FormInstanceId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    settings = settings.Replace("[[EnvironmentId]]", cmd.Parameters["@EnvironmentId"].Value.ToString());
                    settings = settings.Replace("[[SystemActionInstanceId]]", systemActionInstanceId.ToString());
                    settings = settings.Replace("[[FormInstanceId]]", cmd.Parameters["@FormInstanceId"].Value.ToString());
                }
            }

            start = new TemplateCall(Configuration).ExecuteFormInstanceRequest(settings, Token);

            SetConfiguration(systemActionInstanceId, settings);

            return start;
        }
    }
}
