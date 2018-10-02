using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using Undani.Tracking.Execution.Core.Infra;

namespace Undani.Tracking.Execution.Core
{
    public static class FlowInstanceHelper
    {
        public static FlowInstance Get(Guid userId, Guid flowInstanceRefId)
        {
            FlowInstance flow;
            using (SqlConnection cn = new SqlConnection(Configuration.GetValue("ConnectionString:Tracking")))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_FlowInstance", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceRefId", SqlDbType.UniqueIdentifier) { Value = flowInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count == 1)
                        {
                            DataRow dr = dt.Rows[0];
                            ExpandoObjectConverter expandoConverter = new ExpandoObjectConverter();
                            flow = new FlowInstance()
                            {
                                RefId = flowInstanceRefId,
                                FlowName = (string)dr["FlowName"],
                                Key = (string)dr["Key"],
                                Content = JsonConvert.DeserializeObject<ExpandoObject>((string)dr["Content"], expandoConverter),
                                EnvironmentId = (Guid)dr["EnvironmentId"],
                                Created = (DateTime)dr["Created"],
                                States = JsonConvert.DeserializeObject<ExpandoObject>((string)dr["States"], expandoConverter),
                                ActivityInstances = ActivityInstanceHelper.GetSummaryLog(userId, null, (int)dr["Id"])
                            };
                        }
                        else
                            throw new Exception("Could not get the activity");
                    }
                }
            }

            return flow;
        }

        public static FlowInstanceSummary GetSummary(int flowInstanceId)
        {
            FlowInstanceSummary flowInstanceSummary = new FlowInstanceSummary();
            using (SqlConnection cn = new SqlConnection(Configuration.GetValue("ConnectionString:Tracking")))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_FlowInstanceSummary", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceId", SqlDbType.Int) { Value = flowInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceRefId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceKey", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceName", SqlDbType.VarChar, 250) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceRefId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowName", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceKey", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceContent", SqlDbType.VarChar, 2000) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceCreated", SqlDbType.DateTime) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    if (cmd.Parameters["@FlowInstanceRefId"].Value.ToString() != "")
                    {

                        flowInstanceSummary.RefId = (Guid)cmd.Parameters["@FlowInstanceRefId"].Value;
                        flowInstanceSummary.Name = (string)cmd.Parameters["@FlowName"].Value;
                        flowInstanceSummary.Key = (string)cmd.Parameters["@FlowInstanceKey"].Value;
                        flowInstanceSummary.Content = JsonConvert.DeserializeObject<ExpandoObject>((string)cmd.Parameters["@FlowInstanceContent"].Value, new ExpandoObjectConverter());
                        flowInstanceSummary.Created = (DateTime)cmd.Parameters["@FlowInstanceCreated"].Value;

                        if (cmd.Parameters["@ProcedureInstanceRefId"].Value != DBNull.Value)
                        {
                            flowInstanceSummary.ProcedureInstance.RefId = (Guid)cmd.Parameters["@ProcedureInstanceRefId"].Value;
                            flowInstanceSummary.ProcedureInstance.Key = (string)cmd.Parameters["@ProcedureInstanceKey"].Value;
                            flowInstanceSummary.ProcedureInstance.Name = (string)cmd.Parameters["@ProcedureInstanceName"].Value;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return flowInstanceSummary;
        }

        public static FlowInstanceSummary GetSummaryByFormInstanceId(Guid formInstanceId)
        {
            FlowInstanceSummary flowInstanceSummary = new FlowInstanceSummary();
            using (SqlConnection cn = new SqlConnection(Configuration.GetValue("ConnectionString:Tracking")))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_FlowInstanceSummaryFormInstanceId", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@FormInstanceId", SqlDbType.UniqueIdentifier) { Value = formInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceRefId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceKey", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceName", SqlDbType.VarChar, 250) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceRefId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowName", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceKey", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceContent", SqlDbType.VarChar, 2000) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceCreated", SqlDbType.DateTime) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    if (cmd.Parameters["@FlowInstanceRefId"].Value.ToString() != "")
                    {

                        flowInstanceSummary.RefId = (Guid)cmd.Parameters["@FlowInstanceRefId"].Value;
                        flowInstanceSummary.Name = (string)cmd.Parameters["@FlowName"].Value;
                        flowInstanceSummary.Key = (string)cmd.Parameters["@FlowInstanceKey"].Value;
                        flowInstanceSummary.Content = JsonConvert.DeserializeObject<ExpandoObject>((string)cmd.Parameters["@FlowInstanceContent"].Value, new ExpandoObjectConverter());
                        flowInstanceSummary.Created = (DateTime)cmd.Parameters["@FlowInstanceCreated"].Value;                 

                        if (cmd.Parameters["@ProcedureInstanceRefId"].Value != DBNull.Value)
                        {
                            flowInstanceSummary.ProcedureInstance.RefId = (Guid)cmd.Parameters["@ProcedureInstanceRefId"].Value;
                            flowInstanceSummary.ProcedureInstance.Key = (string)cmd.Parameters["@ProcedureInstanceKey"].Value;
                            flowInstanceSummary.ProcedureInstance.Name = (string)cmd.Parameters["@ProcedureInstanceName"].Value;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return flowInstanceSummary;
        }
        
        public static FlowInstanceSummary Create(
            Guid userId, 
            Guid flowRefId, 
            Guid environmentId, 
            string content,
            Guid? activityInstanceRefId,
            string version)
        {
            int flowInstanceId = 0;

            using (SqlConnection cn = new SqlConnection(Configuration.GetValue("ConnectionString:Tracking")))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Create_FlowInstance", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
                    cmd.Parameters.Add(new SqlParameter("@FlowRefId", SqlDbType.UniqueIdentifier) { Value = flowRefId });
                    cmd.Parameters.Add(new SqlParameter("@Version", SqlDbType.VarChar, 50) { Value = version });
                    cmd.Parameters.Add(new SqlParameter("@EnvironmentId", SqlDbType.UniqueIdentifier) { Value = environmentId });
                    cmd.Parameters.Add(new SqlParameter("@Content", SqlDbType.VarChar, 2000) { Value = content });
                    cmd.Parameters.Add(new SqlParameter("@ActivityInstanceRefId", SqlDbType.UniqueIdentifier) { Value = activityInstanceRefId.HasValue ? activityInstanceRefId.Value : Guid.Empty });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceId", SqlDbType.Int) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    if (cmd.Parameters["@FlowInstanceId"].Value == DBNull.Value)
                        throw new Exception("Insufficient information to create the flow");

                    flowInstanceId = (int)cmd.Parameters["@FlowInstanceId"].Value;                    
                }
            }            

            ActivityInstanceHelper.Create(userId, flowInstanceId);            

            return GetSummary(flowInstanceId);
        }

        public static FlowInstanceSummary CreateByFormInstanceId(
            Guid userId,
            Guid flowRefId,
            Guid environmentId,
            string content,
            string formInstanceKey,
            Guid formInstanceId,
            Guid? activityInstanceRefId,
            string version)
        {
            int flowInstanceId = 0;

            FlowInstanceSummary flowInstanceSummary = GetSummaryByFormInstanceId(formInstanceId);

            if (flowInstanceSummary == null)
            {
                using (SqlConnection cn = new SqlConnection(Configuration.GetValue("ConnectionString:Tracking")))
                {
                    cn.Open();

                    using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Create_FlowInstanceFormInstanceId", cn) { CommandType = CommandType.StoredProcedure })
                    {
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@FlowRefId", SqlDbType.UniqueIdentifier) { Value = flowRefId });
                        cmd.Parameters.Add(new SqlParameter("@Version", SqlDbType.VarChar, 50) { Value = version });
                        cmd.Parameters.Add(new SqlParameter("@EnvironmentId", SqlDbType.UniqueIdentifier) { Value = environmentId });
                        cmd.Parameters.Add(new SqlParameter("@Content", SqlDbType.VarChar, 2000) { Value = content });
                        cmd.Parameters.Add(new SqlParameter("@FormInstanceKey", SqlDbType.VarChar, 50) { Value = formInstanceKey });
                        cmd.Parameters.Add(new SqlParameter("@FormInstanceId", SqlDbType.UniqueIdentifier) { Value = formInstanceId });
                        cmd.Parameters.Add(new SqlParameter("@ActivityInstanceRefId", SqlDbType.UniqueIdentifier) { Value = activityInstanceRefId.HasValue ? activityInstanceRefId.Value : Guid.Empty });
                        cmd.Parameters.Add(new SqlParameter("@FlowInstanceId", SqlDbType.Int) { Direction = ParameterDirection.Output });

                        cmd.ExecuteNonQuery();

                        if (cmd.Parameters["@FlowInstanceId"].Value == DBNull.Value)
                            throw new Exception("Insufficient information to create the flow");

                        flowInstanceId = (int)cmd.Parameters["@FlowInstanceId"].Value;

                        flowInstanceSummary = GetSummary(flowInstanceId);
                    }
                }

                ActivityInstanceHelper.Create(userId, flowInstanceId);
            }

            return flowInstanceSummary;
        }

        public static void SetUserGroup(Guid userId, Guid flowIntanceRefId, UserGroup[] responsibles)
        {
            using (SqlConnection cn = new SqlConnection(Configuration.GetValue("ConnectionString:Tracking")))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandText = "EXECUTION.usp_Delete_UserGroup";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId } );
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceRefId", SqlDbType.UniqueIdentifier) { Value = flowIntanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceId", SqlDbType.Int) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "EXECUTION.usp_Set_UserGroup";
                    cmd.Parameters.RemoveAt("@FlowInstanceRefId");
                    cmd.Parameters["@FlowInstanceId"].Direction = ParameterDirection.Input;

                    cmd.Parameters.Add(new SqlParameter("@AddUserId", SqlDbType.UniqueIdentifier));
                    cmd.Parameters.Add(new SqlParameter("@Order", SqlDbType.Int));
                    foreach (UserGroup responsible in responsibles)
                    {
                        cmd.Parameters["@AddUserId"].Value = responsible.UserId;
                        cmd.Parameters["@Order"].Value = responsible.Order;

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public static void SetUserGroupFormInstance(Guid userId, Guid formIntanceId, UserGroup[] responsibles)
        {
            using (SqlConnection cn = new SqlConnection(Configuration.GetValue("ConnectionString:Tracking")))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandText = "EXECUTION.usp_Delete_UserGroupFormInstanceId";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
                    cmd.Parameters.Add(new SqlParameter("@FormInstanceId", SqlDbType.UniqueIdentifier) { Value = formIntanceId });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceId", SqlDbType.Int) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "EXECUTION.usp_Set_UserGroup";
                    cmd.Parameters.RemoveAt("@FormInstanceId");
                    cmd.Parameters["@FlowInstanceId"].Direction = ParameterDirection.Input;

                    cmd.Parameters.Add(new SqlParameter("@AddUserId", SqlDbType.UniqueIdentifier));
                    cmd.Parameters.Add(new SqlParameter("@Order", SqlDbType.Int));
                    foreach (UserGroup responsible in responsibles)
                    {
                        cmd.Parameters["@AddUserId"].Value = responsible.UserId;
                        cmd.Parameters["@Order"].Value = responsible.Order;

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public static dynamic SetContentProperty(Guid userId, Guid flowInstanceRefId, string propertyName, string value)
        {
            string content;
            using (SqlConnection cn = new SqlConnection(Configuration.GetValue("ConnectionString:Tracking")))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_ContentProperty", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceRefId", SqlDbType.UniqueIdentifier) { Value = flowInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@PropertyName", SqlDbType.VarChar, 50) { Value = propertyName });
                    cmd.Parameters.Add(new SqlParameter("@Value", SqlDbType.VarChar, 250) { Value = value });
                    cmd.Parameters.Add(new SqlParameter("@Content", SqlDbType.VarChar, 2000) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    content = (string)cmd.Parameters["@Content"].Value;
                }
            }

            return JsonConvert.DeserializeObject<ExpandoObject>(content, new ExpandoObjectConverter());
        }

        public static dynamic SetContentPropertyFormInstance(Guid userId, Guid formInstanceId, string propertyName, string value)
        {
            string content;
            using (SqlConnection cn = new SqlConnection(Configuration.GetValue("ConnectionString:Tracking")))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_ContentPropertyFormInstanceId", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
                    cmd.Parameters.Add(new SqlParameter("@FormInstanceId", SqlDbType.UniqueIdentifier) { Value = formInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@PropertyName", SqlDbType.VarChar, 50) { Value = propertyName });
                    cmd.Parameters.Add(new SqlParameter("@Value", SqlDbType.VarChar, 250) { Value = value });
                    cmd.Parameters.Add(new SqlParameter("@Content", SqlDbType.VarChar, 2000) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    content = (string)cmd.Parameters["@Content"].Value;
                }
            }

            return JsonConvert.DeserializeObject<ExpandoObject>(content, new ExpandoObjectConverter());
        }

        public static void SetState(Guid userId, Guid activityInstanceRefId, string key, string state)
        {
            using (SqlConnection cn = new SqlConnection(Configuration.GetValue("ConnectionString:Tracking")))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_StateFlowInstance", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
                    cmd.Parameters.Add(new SqlParameter("@ActivityInstanceRefId", SqlDbType.UniqueIdentifier) { Value = activityInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@Key", SqlDbType.VarChar, 50) { Value = key });
                    cmd.Parameters.Add(new SqlParameter("@State", SqlDbType.VarChar, 50) { Value = state });
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void SetStateFormInstance(Guid userId, Guid formInstanceId, string key, string state)
        {
            using (SqlConnection cn = new SqlConnection(Configuration.GetValue("ConnectionString:Tracking")))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_StateFlowInstanceFormInstanceId", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
                    cmd.Parameters.Add(new SqlParameter("@FormInstanceId", SqlDbType.UniqueIdentifier) { Value = formInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@Key", SqlDbType.VarChar, 50) { Value = key });
                    cmd.Parameters.Add(new SqlParameter("@State", SqlDbType.VarChar, 50) { Value = state });
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static PagedList<FlowInstanceSummary> GetLog(Guid userId, int? pageLimit = null, int? page = null)
        {
            List<FlowInstanceSummary> flowLog = new List<FlowInstanceSummary>();
            using (SqlConnection cn = new SqlConnection(Configuration.GetValue("ConnectionString:Tracking")))
            {
                cn.Open();
                
                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_FlowInstanceSummaryLog", cn);
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
                cmd.CommandType = CommandType.StoredProcedure;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    ExpandoObjectConverter expandoConverter = new ExpandoObjectConverter();
                    FlowInstanceSummary flowInstanceSummary;
                    while (reader.Read())
                    {
                        flowInstanceSummary = new FlowInstanceSummary()
                        {
                            RefId = reader.GetGuid(0),
                            Name = reader.GetString(1),
                            Key = reader.GetString(2),
                            Content = JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(3), expandoConverter),
                            Created = reader.GetDateTime(4)
                        };

                        if (!reader.IsDBNull(5))
                        {
                            flowInstanceSummary.ProcedureInstance.RefId = reader.GetGuid(5);
                            flowInstanceSummary.ProcedureInstance.Key = reader.GetString(6);
                            flowInstanceSummary.ProcedureInstance.Name = reader.GetString(7);
                        }

                        flowLog.Add(flowInstanceSummary);
                    }
                }
            }

            return new PagedList<FlowInstanceSummary>(flowLog.AsQueryable(), page, pageLimit);
        }
    }
}
