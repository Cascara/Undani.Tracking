using System;
using System.Data;
using System.Data.SqlClient;

namespace Undani.Tracking.Core.Invoke
{
    public partial class SystemActionInvoke
    {
        public bool KeyCalculation(Guid systemActionInstanceId, string alias, string settings)
        {
            bool start = false;
            switch (alias)
            {
                case "FlowInstanceKey":
                    start = FlowInstanceKey(systemActionInstanceId, settings);
                    break;

                case "ProcedureInstanceKey":
                    start = ProcedureInstanceKey(systemActionInstanceId, settings);
                    break;

                default:
                    throw new NotImplementedException();
            }

            return start;
        }

        private bool FlowInstanceKey(Guid systemActionInstanceId, string settings)
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

            SetConfiguration(systemActionInstanceId, settings);

            return start;
        }

        private bool ProcedureInstanceKey(Guid systemActionInstanceId, string settings)
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

            SetConfiguration(systemActionInstanceId, settings);

            return start;
        }
    }
}
