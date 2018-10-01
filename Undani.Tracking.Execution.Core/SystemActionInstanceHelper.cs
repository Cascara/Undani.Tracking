using System;
using System.Data;
using System.Data.SqlClient;

namespace Undani.Tracking.Execution.Core
{
    public static class SystemActionInstanceHelper
    {
        public static void Execute(Guid systemActionInstanceId)
        {
            using (SqlConnection cn = new SqlConnection(Configuration.GetValue("ConnectionString:Tracking")))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_SystemActionInstance", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@ActionInstanceId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@SystemActionTypeMethod", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@SystemActionAlias", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    Invoke((Guid)cmd.Parameters["@ActionInstanceId"].Value, (string)cmd.Parameters["@SystemActionTypeMethod"].Value, (string)cmd.Parameters["@SystemActionAlias"].Value);

                }
            }
        }

        private static void Invoke(Guid systemActionInstanceId, string method, string alias)
        {
            typeof(SystemActionInvoke).GetMethod(method).Invoke(null, new object[] { systemActionInstanceId, alias });
        }

        public static void Finish(Guid systemActionInstanceId)
        {
            using (SqlConnection cn = new SqlConnection(Configuration.GetValue("ConnectionString:Tracking")))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Finish_SystemActionInstance", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.InputOutput, Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@ActionInstanceId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    if (cmd.Parameters["@SystemActionInstanceId"].Value != DBNull.Value)
                        Execute((Guid)cmd.Parameters["@SystemActionInstanceId"].Value);
                    else
                        ActionInstanceHelper.Finish((Guid)cmd.Parameters["@ActionInstanceId"].Value);
                }
            }
        }
    }
}
