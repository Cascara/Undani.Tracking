using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;

namespace Undani.Tracking.Execution.Core
{
    public class FlowInstanceHelper : Helper
    {
        public FlowInstanceHelper(IConfiguration configuration, Guid userId, string token = "") : base(configuration, userId, token) { }

        public FlowInstance Get(Guid flowInstanceRefId)
        {
            FlowInstance flow;
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_FlowInstance", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceRefId", SqlDbType.UniqueIdentifier) { Value = flowInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });

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
                                ActivityInstances = new ActivityInstanceHelper(Configuration, UserId, Token).GetSummaryLog(null, (int)dr["Id"])
                            };
                        }
                        else
                            throw new Exception("Could not get the activity");
                    }
                }
            }

            return flow;
        }

        public FlowInstanceSummary GetSummary(int flowInstanceId)
        {
            FlowInstanceSummary flowInstanceSummary = new FlowInstanceSummary();
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_FlowInstanceSummary", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceId", SqlDbType.Int) { Value = flowInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceRefId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceKey", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceName", SqlDbType.VarChar, 250) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceContent", SqlDbType.VarChar, 2000) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceStartDate", SqlDbType.DateTime) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceEnvironmentId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceRefId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowName", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceKey", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceContent", SqlDbType.VarChar, 2000) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceCreated", SqlDbType.DateTime) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    if (cmd.Parameters["@FlowInstanceRefId"].Value.ToString() != "")
                    {
                        ExpandoObjectConverter expandoObjectConverter = new ExpandoObjectConverter();

                        flowInstanceSummary.RefId = (Guid)cmd.Parameters["@FlowInstanceRefId"].Value;
                        flowInstanceSummary.Name = (string)cmd.Parameters["@FlowName"].Value;
                        flowInstanceSummary.Key = (string)cmd.Parameters["@FlowInstanceKey"].Value;
                        flowInstanceSummary.Content = JsonConvert.DeserializeObject<ExpandoObject>((string)cmd.Parameters["@FlowInstanceContent"].Value, expandoObjectConverter);
                        flowInstanceSummary.Created = (DateTime)cmd.Parameters["@FlowInstanceCreated"].Value;

                        flowInstanceSummary.ProcedureInstanceSummary.RefId = (Guid)cmd.Parameters["@ProcedureInstanceRefId"].Value;
                        flowInstanceSummary.ProcedureInstanceSummary.Name = (string)cmd.Parameters["@ProcedureInstanceName"].Value;
                        flowInstanceSummary.ProcedureInstanceSummary.Key = (string)cmd.Parameters["@ProcedureInstanceKey"].Value;
                        flowInstanceSummary.ProcedureInstanceSummary.Content = JsonConvert.DeserializeObject<ExpandoObject>((string)cmd.Parameters["@ProcedureInstanceContent"].Value, expandoObjectConverter);
                        if(cmd.Parameters["@ProcedureInstanceStartDate"].Value != DBNull.Value)
                            flowInstanceSummary.ProcedureInstanceSummary.Start = (DateTime)cmd.Parameters["@ProcedureInstanceStartDate"].Value;
                        flowInstanceSummary.ProcedureInstanceSummary.EnvironmentId = (Guid)cmd.Parameters["@ProcedureInstanceEnvironmentId"].Value;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return flowInstanceSummary;
        }

        public FlowInstanceSummary GetSummaryByFormInstanceId(Guid formInstanceId)
        {
            FlowInstanceSummary flowInstanceSummary = new FlowInstanceSummary();
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
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
                            flowInstanceSummary.ProcedureInstanceSummary.RefId = (Guid)cmd.Parameters["@ProcedureInstanceRefId"].Value;
                            flowInstanceSummary.ProcedureInstanceSummary.Key = (string)cmd.Parameters["@ProcedureInstanceKey"].Value;
                            //flowInstanceSummary.ProcedureInstance.Name = (string)cmd.Parameters["@ProcedureInstanceName"].Value;
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

        public Guid Create(int flowId, int procedureInstanceId, int activityInstanceId, string version = "")
        {
            int flowInstanceId = 0;
            string activityId = "";

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Create_FlowInstance", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.Add(new SqlParameter("@FlowId", SqlDbType.Int) { Value = flowId });
                    cmd.Parameters.Add(new SqlParameter("@Version", SqlDbType.VarChar, 50) { Value = version });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceId", SqlDbType.Int) { Value = procedureInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@ActivityInstanceId", SqlDbType.Int) { Value = activityInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceId", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ActivityId", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    flowInstanceId = (int)cmd.Parameters["@FlowInstanceId"].Value;
                    activityId = (string)cmd.Parameters["@ActivityId"].Value;
                }
            }                      

            return new ActivityInstanceHelper(Configuration, UserId, Token).Create(activityId, flowInstanceId);
        }


        public void SetUserGroup(Guid flowIntanceRefId, UserGroup[] responsibles)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandText = "EXECUTION.usp_Delete_UserGroup";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId } );
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

        public void SetUserGroupFormInstance(Guid formIntanceId, UserGroup[] responsibles)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandText = "EXECUTION.usp_Delete_UserGroupFormInstanceId";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
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

        public dynamic SetContentProperty(Guid flowInstanceRefId, string propertyName, string value)
        {
            string content;
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_ContentProperty", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
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

        public dynamic SetContentPropertyFormInstance(Guid formInstanceId, string propertyName, string value)
        {
            string content;
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_ContentPropertyFormInstanceId", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
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

        public void SetState(Guid activityInstanceRefId, string key, string state)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_StateFlowInstance", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@ActivityInstanceRefId", SqlDbType.UniqueIdentifier) { Value = activityInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@Key", SqlDbType.VarChar, 50) { Value = key });
                    cmd.Parameters.Add(new SqlParameter("@State", SqlDbType.VarChar, 50) { Value = state });
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void SetStateFormInstance(Guid formInstanceId, string key, string state)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_StateFlowInstanceFormInstanceId", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@FormInstanceId", SqlDbType.UniqueIdentifier) { Value = formInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@Key", SqlDbType.VarChar, 50) { Value = key });
                    cmd.Parameters.Add(new SqlParameter("@State", SqlDbType.VarChar, 50) { Value = state });
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public PagedList<FlowInstanceSummary> GetLog(int? pageLimit = null, int? page = null)
        {
            List<FlowInstanceSummary> flowLog = new List<FlowInstanceSummary>();
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();
                
                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_FlowInstanceSummaryLog", cn);
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
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
                            flowInstanceSummary.ProcedureInstanceSummary.RefId = reader.GetGuid(5);
                            flowInstanceSummary.ProcedureInstanceSummary.Key = reader.GetString(6);
                            //flowInstanceSummary.ProcedureInstance.Name = reader.GetString(7);
                        }

                        flowLog.Add(flowInstanceSummary);
                    }
                }
            }

            return new PagedList<FlowInstanceSummary>(flowLog.AsQueryable(), page, pageLimit);
        }
    }
}
