using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Reflection;
using Undani.Tracking.Core.Invoke;
using Undani.Tracking.Execution.Core.Resource;

namespace Undani.Tracking.Execution.Core
{
    public class SystemActionInstanceHelper : Helper
    {
        public SystemActionInstanceHelper(IConfiguration configuration, Guid userId, string token = "") : base(configuration, userId, token) { }

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
                    cmd.Parameters.Add(new SqlParameter("@SystemActionSettings", SqlDbType.VarChar, 2000) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@SystemActionAsynchronous", SqlDbType.Bit) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@SystemActionStrict", SqlDbType.Bit) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    string method = (string)cmd.Parameters["@SystemActionTypeMethod"].Value;
                    string alias = (string)cmd.Parameters["@SystemActionAlias"].Value;
                    string settings = (string)cmd.Parameters["@SystemActionSettings"].Value;

                    bool isAsynchronous = (bool)cmd.Parameters["@SystemActionAsynchronous"].Value;
                    bool isStrict = (bool)cmd.Parameters["@SystemActionStrict"].Value;

                    bool startCorrect = Start(systemActionInstanceId, method, alias, settings, isStrict);

                    if (isAsynchronous)
                    {
                        if (isStrict)
                        {
                            if (!startCorrect)
                            {
                                throw new Exception("It was not possible to start the system action correctly.");
                            }
                        }
                        else
                        {
                            Finish(systemActionInstanceId);
                        }
                        
                    }
                    else
                    {
                        if (startCorrect)
                        {
                            Finish(systemActionInstanceId);
                        }
                        else
                        {
                            if (isStrict)
                            {
                                if (!alias.Contains("EvaluateDaily"))
                                {
                                    throw new Exception("It was not possible to start the system action correctly.");
                                }                                
                            }
                            else
                            {
                                Finish(systemActionInstanceId);
                            }
                        }
                    }
                }
            }
        }

        public bool Start(Guid systemActionInstanceId, string method, string alias, string settings, bool isStrict)
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

                    invokedCorrect = new SystemActionInvoke(Configuration, UserId, Token).Invoke(systemActionInstanceId, method, alias, settings, isStrict);
                }
            }

            return invokedCorrect;
        }

        public void Finish(Guid systemActionInstanceId, string procedureInstanceContent = "{}", string flowInstanceContent = "{}")
        {
            if (systemActionInstanceId == Guid.Empty)
                throw new Exception("The parameter can not be empty");

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SystemActionInstanceFinish", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceContent", SqlDbType.VarChar, -1) { Value = procedureInstanceContent });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceContent", SqlDbType.VarChar, -1) { Value = flowInstanceContent });
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.InputOutput, Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@ActionInstanceId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();


                    if (cmd.Parameters["@SystemActionInstanceId"].Value != DBNull.Value)
                    {
                        if ((Guid)cmd.Parameters["@SystemActionInstanceId"].Value != Guid.Empty)
                        {
                            Execute((Guid)cmd.Parameters["@SystemActionInstanceId"].Value);
                        }                        
                    }                        
                    else
                    {
                        ActionInstanceHelper actionInstanceHelper = new ActionInstanceHelper(Configuration, UserId, Token);
                        actionInstanceHelper.Finish((Guid)cmd.Parameters["@ActionInstanceId"].Value);
                    }
                    
                        
                }
            }
        }

        public void ExecuteDaily()
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_SystemActionInstanceExecuteDaily", cn) { CommandType = CommandType.StoredProcedure };

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Execute(reader.GetGuid(0));
                    }
                }
            }
        }

        public void FowardMessage(string queueName, string message)
        {
            BusCall busCall = new BusCall(Configuration);
            busCall.SendMessage(queueName, message);
        }
    }
}
