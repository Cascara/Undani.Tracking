﻿using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
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
                                ActivityInstances = GetLog((int)dr["Id"])
                            };
                        }
                        else
                            throw new Exception("Could not get the activity");
                    }
                }
            }

            return flow;
        }

        public List<ActivityInstanceSummary> GetLog(int flowInstanceId)
        {
            List<ActivityInstanceSummary> activityLog = new List<ActivityInstanceSummary>();
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_FlowInstanceLog", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@FlowInstanceId", SqlDbType.Int) { Value = flowInstanceId });

                return new ActivityInstanceHelper(Configuration, UserId, Token).FillActivitiesInstanceSummary(cmd);
            }

            
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
                    cmd.Parameters.Add(new SqlParameter("@ProcedureRefId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceKey", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceName", SqlDbType.VarChar, 250) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceContent", SqlDbType.VarChar, 2000) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceStartDate", SqlDbType.DateTime) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceEnvironmentId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstancePrincipalState", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureDocumentsSigned", SqlDbType.VarChar, 8000) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceRefId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowRefId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowName", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceKey", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceContent", SqlDbType.VarChar, 2000) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceCreated", SqlDbType.DateTime) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    if (cmd.Parameters["@FlowInstanceRefId"].Value.ToString() != "")
                    {
                        ExpandoObjectConverter expandoObjectConverter = new ExpandoObjectConverter();

                        flowInstanceSummary.RefId = (Guid)cmd.Parameters["@FlowInstanceRefId"].Value;
                        flowInstanceSummary.FlowRefId = (Guid)cmd.Parameters["@FlowRefId"].Value;
                        flowInstanceSummary.Name = (string)cmd.Parameters["@FlowName"].Value;
                        flowInstanceSummary.Key = (string)cmd.Parameters["@FlowInstanceKey"].Value;
                        flowInstanceSummary.Content = JsonConvert.DeserializeObject<ExpandoObject>((string)cmd.Parameters["@FlowInstanceContent"].Value, expandoObjectConverter);
                        flowInstanceSummary.Created = (DateTime)cmd.Parameters["@FlowInstanceCreated"].Value;

                        flowInstanceSummary.ProcedureInstanceSummary.RefId = (Guid)cmd.Parameters["@ProcedureInstanceRefId"].Value;
                        flowInstanceSummary.ProcedureInstanceSummary.ProcedureRefId = (Guid)cmd.Parameters["@ProcedureRefId"].Value;
                        flowInstanceSummary.ProcedureInstanceSummary.Name = (string)cmd.Parameters["@ProcedureInstanceName"].Value;
                        flowInstanceSummary.ProcedureInstanceSummary.Key = (string)cmd.Parameters["@ProcedureInstanceKey"].Value;
                        flowInstanceSummary.ProcedureInstanceSummary.Content = JsonConvert.DeserializeObject<ExpandoObject>((string)cmd.Parameters["@ProcedureInstanceContent"].Value, expandoObjectConverter);
                        if(cmd.Parameters["@ProcedureInstanceStartDate"].Value != DBNull.Value)
                            flowInstanceSummary.ProcedureInstanceSummary.Start = (DateTime)cmd.Parameters["@ProcedureInstanceStartDate"].Value;
                        flowInstanceSummary.ProcedureInstanceSummary.EnvironmentId = (Guid)cmd.Parameters["@ProcedureInstanceEnvironmentId"].Value;
                        flowInstanceSummary.ProcedureInstanceSummary.PrincipalState = (string)cmd.Parameters["@ProcedureInstancePrincipalState"].Value;
                        flowInstanceSummary.ProcedureInstanceSummary.DocumentsSignedZiped = GetDocumentsSignedZiped((string)cmd.Parameters["@ProcedureDocumentsSigned"].Value);
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return flowInstanceSummary;
        }

        private string GetDocumentsSignedZiped(string documentsSigned)
        {
            JObject jObject = JObject.Parse(documentsSigned);

            JEnumerable<JToken> jTokens = jObject.Children();

            JArray jArray;
            string documents = "";
            foreach (JToken jToken in jTokens)
            {
                jArray = (JArray)jObject[jToken.Path];
                for (int i = 0; i < jArray.Count; i++)
                {
                    documents += "," + jArray[i]["SystemName"];
                }
            }

            return documents != "" ? documents.Substring(1) : "";
        }

        public Guid Create(int flowId, int procedureInstanceId, Guid? systemActionInstanceId, string version = "")
        {
            int flowInstanceId = 0;
            string elementId = "";

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Create_FlowInstance", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.Add(new SqlParameter("@FlowId", SqlDbType.Int) { Value = flowId });
                    cmd.Parameters.Add(new SqlParameter("@Version", SqlDbType.VarChar, 50) { Value = version });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceId", SqlDbType.Int) { Value = procedureInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId ?? Guid.Empty });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceId", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ElementId", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    flowInstanceId = (int)cmd.Parameters["@FlowInstanceId"].Value;
                    elementId = (string)cmd.Parameters["@ElementId"].Value;
                }
            }                      

            return new ElementInstanceHelper(Configuration, UserId, Token).Create(elementId, flowInstanceId);
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

        public bool SetState(Guid flowInstanceRefId, string key, string state)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_StateFlowInstance", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceRefId", SqlDbType.UniqueIdentifier) { Value = flowInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@Key", SqlDbType.VarChar, 50) { Value = key });
                    cmd.Parameters.Add(new SqlParameter("@State", SqlDbType.VarChar, 50) { Value = state });
                    cmd.ExecuteNonQuery();
                }
            }

            return true;
        }

        public dynamic GetState(Guid flowInstanceRefId)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_StateFlowInstanceRefId", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceRefId", SqlDbType.UniqueIdentifier) { Value = flowInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@States", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();

                    return JsonConvert.DeserializeObject<ExpandoObject>((string)cmd.Parameters["@States"].Value, new ExpandoObjectConverter());
                }
            }
        }
    }
}
