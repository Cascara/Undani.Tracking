﻿using Microsoft.Extensions.Configuration;
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
        public bool State(Guid systemActionInstanceId, string alias, string configuration)
        {
            bool start = false;
            switch (alias)
            {
                case "FlowInstance":
                    start = SetStateFlowInstance(systemActionInstanceId, configuration);
                    break;

                case "ProcedureInstance":
                    start = SetStateProcedureInstance(systemActionInstanceId, configuration);
                    break;

                default:
                    throw new Exception("The method is not implemented");
            }

            return start;
        }

        private bool SetStateFlowInstance(Guid systemActionInstanceId, string configuration)
        {
            bool start = false;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SystemActionStateFlowInstance", cn))
                {
                    dynamic stateFlowInstance = JsonConvert.DeserializeObject<ExpandoObject>(configuration, new ExpandoObjectConverter());

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@Key", SqlDbType.VarChar, 50) { Value = stateFlowInstance.Key });
                    cmd.Parameters.Add(new SqlParameter("@State", SqlDbType.VarChar, 50) { Value = stateFlowInstance.State });

                    cmd.ExecuteNonQuery();
                    start = true;
                }
            }                       

            return start;
        }

        private bool SetStateProcedureInstance(Guid systemActionInstanceId, string configuration)
        {
            bool start = false;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SystemActionStateProcedureInstance", cn))
                {
                    dynamic stateProcedureInstance = JsonConvert.DeserializeObject<ExpandoObject>(configuration, new ExpandoObjectConverter());

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@Key", SqlDbType.VarChar, 50) { Value = stateProcedureInstance.Key });
                    cmd.Parameters.Add(new SqlParameter("@State", SqlDbType.VarChar, 50) { Value = stateProcedureInstance.State });

                    cmd.ExecuteNonQuery();
                    start = true;
                }
            }               

            return start;
        }
    }
}
