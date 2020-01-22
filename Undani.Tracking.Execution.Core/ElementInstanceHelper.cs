using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text;

namespace Undani.Tracking.Execution.Core
{
    public class ElementInstanceHelper : Helper
    {
        public ElementInstanceHelper(IConfiguration configuration, Guid userId, string token = "") : base(configuration, userId, token) { }

        public Guid Create(string elementId, int flowInstanceId)
        {
            return Create(elementId, flowInstanceId, Guid.Empty);
        }

        public Guid Create(Guid actionInstanceId)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ElementActionInstance", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.Add(new SqlParameter("@ActionInstanceId", SqlDbType.UniqueIdentifier) { Value = actionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceId", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ElementId", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    string elementId = (string)cmd.Parameters["@ElementId"].Value;
                    int flowInstanceId = (int)cmd.Parameters["@FlowInstanceId"].Value;

                    return Create(elementId, flowInstanceId, actionInstanceId);
                }
            }
        }

        private Guid Create(string elementId, int flowInstanceId, Guid actionInstanceId)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Create_ElementInstance", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.Add(new SqlParameter("@ElementId", SqlDbType.VarChar, 50) { Value = elementId });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceId", SqlDbType.Int) { Value = flowInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@ActionInstanceId", SqlDbType.UniqueIdentifier) { Value = actionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@ElementInstanceId", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ElementInstanceRefId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ElementTypeId", SqlDbType.Int) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    switch ((int)cmd.Parameters["@ElementTypeId"].Value)
                    {
                        case 1:
                            ActivityInstanceHelper activityInstanceHelper = new ActivityInstanceHelper(Configuration, UserId, Token);
                            activityInstanceHelper.Create(elementId, (int)cmd.Parameters["@ElementInstanceId"].Value);
                            break;

                        case 2:
                            ///TODO: Crear la funcionalidad de GATEWAY
                            break;
                    }

                    return (Guid)cmd.Parameters["@ElementInstanceRefId"].Value; ;
                }
            }
            
        }
    }
}
