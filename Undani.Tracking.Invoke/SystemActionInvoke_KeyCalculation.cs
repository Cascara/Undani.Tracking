using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Text;
using Undani.Tracking.Invoke.Resource;
using Undani.Tracking.Invoke.Infra;
using System.Data;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Undani.Tracking.Invoke
{
    public partial class SystemActionInvoke
    {
        public bool KeyCalculation(Guid systemActionInstanceId, string alias, string configuration)
        {
            bool start = false;
            switch (alias)
            {
                case "FlowInstance":
                    start = SetFlowInstanceKey(systemActionInstanceId);
                    break;

                case "ProcedureInstance":
                    start = SetProcedureInstanceKey(systemActionInstanceId);
                    break;

                default:
                    throw new Exception("The method is not implemented");
            }

            return start;
        }

        private bool SetFlowInstanceKey(Guid systemActionInstanceId)
        {
            bool start = false;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SystemActionFlowInstanceKey", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });

                    cmd.ExecuteNonQuery();
                    start = true;
                }
            }                       

            return start;
        }

        private bool SetProcedureInstanceKey(Guid systemActionInstanceId)
        {
            bool start = false;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SystemActionProcedureInstanceKey", cn))
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
