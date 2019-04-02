using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Text;
using Undani.Tracking.Core.Invoke.Resource;
using Undani.Tracking.Core.Invoke.Infra;
using System.Data;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Undani.Tracking.Core.Invoke
{
    public partial class SystemActionInvoke
    {
        public bool KeyCalculation(Guid systemActionInstanceId, string alias, string configuration)
        {
            bool start = false;
            switch (alias)
            {
                case "FlowInstanceKey":
                    start = FlowInstanceKey(systemActionInstanceId);
                    break;

                case "ProcedureInstanceKey":
                    start = ProcedureInstanceKey(systemActionInstanceId);
                    break;

                default:
                    throw new Exception("The method is not implemented");
            }

            return start;
        }

        private bool FlowInstanceKey(Guid systemActionInstanceId)
        {
            bool start = false;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_FlowInstanceKey", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });

                    cmd.ExecuteNonQuery();
                    start = true;
                }
            }                       

            return start;
        }

        private bool ProcedureInstanceKey(Guid systemActionInstanceId)
        {
            bool start = false;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_ProcedureInstanceKey", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });

                    cmd.ExecuteNonQuery();
                    start = true;
                }
            }               

            return start;
        }
    }
}
