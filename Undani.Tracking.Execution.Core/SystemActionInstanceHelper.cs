using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using Undani.Tracking.Invoke;

namespace Undani.Tracking.Execution.Core
{
    public class SystemActionInstanceHelper : Helper
    {
        public SystemActionInstanceHelper(IConfiguration configuration) : base(configuration) { }

        public void Execute(Guid systemActionInstanceId)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_SystemActionInstance", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@SystemActionTypeMethod", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@SystemActionAlias", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@SystemActionConfiguration", SqlDbType.VarChar, 2000) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@SystemActionAsynchronous", SqlDbType.Bit) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    if (Start(systemActionInstanceId, (string)cmd.Parameters["@SystemActionTypeMethod"].Value, (string)cmd.Parameters["@SystemActionAlias"].Value, (string)cmd.Parameters["@SystemActionConfiguration"].Value))
                    {
                        if (!(bool)cmd.Parameters["@SystemActionAsynchronous"].Value)
                        {
                            Finish(systemActionInstanceId);
                        }
                    }
                }
            }
        }

        public bool Start(Guid systemActionInstanceId, string method, string alias, string configuration)
        {
            bool invokedCorrect = false;
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SystemActionInstanceStart", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });

                    cmd.ExecuteNonQuery();

                    invokedCorrect = new SystemActionInvoke(Configuration).Invoke(systemActionInstanceId, method, alias, configuration);
                }
            }

            return invokedCorrect;
        }

        public void Finish(Guid systemActionInstanceId)
        {
            if (systemActionInstanceId == Guid.Empty)
                throw new Exception("The parameter can not be empty");

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SystemActionInstanceFinish", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.InputOutput, Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@ActionInstanceId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    if (cmd.Parameters["@SystemActionInstanceId"].Value != DBNull.Value)
                        Execute((Guid)cmd.Parameters["@SystemActionInstanceId"].Value);
                    else
                    {
                        ActionInstanceHelper actionInstanceHelper = new ActionInstanceHelper(Configuration);
                        actionInstanceHelper.Finish((Guid)cmd.Parameters["@ActionInstanceId"].Value);
                    }
                        
                }
            }
        }
    }
}
