using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Dynamic;

namespace Undani.Tracking.Core.Invoke
{
    public partial class SystemActionInvoke
    {
        public bool State(Guid systemActionInstanceId, string alias, string settings)
        {
            bool start = false;
            switch (alias)
            {
                case "StateFlowInstance":
                    start = StateFlowInstance(systemActionInstanceId, settings);
                    break;

                case "StateProcedureInstance":
                    start = StateProcedureInstance(systemActionInstanceId, settings);
                    break;

                default:
                    throw new NotImplementedException();
            }

            return start;
        }

        private bool StateFlowInstance(Guid systemActionInstanceId, string settings)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_StateFlowInstance", cn))
                {
                    dynamic stateFlowInstance = JsonConvert.DeserializeObject<ExpandoObject>(settings, new ExpandoObjectConverter());

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@Key", SqlDbType.VarChar, 50) { Value = stateFlowInstance.Key });
                    cmd.Parameters.Add(new SqlParameter("@State", SqlDbType.VarChar, 50) { Value = stateFlowInstance.State });

                    cmd.ExecuteNonQuery();
                }
            }

            SetConfiguration(systemActionInstanceId, settings);

            return true;
        }

        private bool StateProcedureInstance(Guid systemActionInstanceId, string settings)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_StateProcedureInstance", cn))
                {
                    dynamic stateProcedureInstance = JsonConvert.DeserializeObject<ExpandoObject>(settings, new ExpandoObjectConverter());

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@Key", SqlDbType.VarChar, 50) { Value = stateProcedureInstance.Key });
                    cmd.Parameters.Add(new SqlParameter("@State", SqlDbType.VarChar, 50) { Value = stateProcedureInstance.State });

                    cmd.ExecuteNonQuery();
                }
            }

            SetConfiguration(systemActionInstanceId, settings);

            return true;
        }
    }
}
