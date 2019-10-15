using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using Undani.Tracking.Execution.Core.Resource;

namespace Undani.Tracking.Execution.Core
{
    public class MessageHelper : Helper
    {
        public MessageHelper(IConfiguration configuration, Guid userId, string token = "") : base(configuration, userId, token) { }

        public OpenedMessage GetOpen(Guid messageId)
        {

            OpenedMessage openedMessage = new OpenedMessage();
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();
                
                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_MessageOpen", cn))
                {

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId, Direction = ParameterDirection.InputOutput });
                    cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@MessageId", SqlDbType.UniqueIdentifier) { Value = messageId });
                    cmd.Parameters.Add(new SqlParameter("@ElementInstanceRefId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();
                    if (cmd.Parameters["@ElementInstanceRefId"].Value.ToString() != "")
                    {
                        openedMessage.ElementInstanceRefId = (Guid)cmd.Parameters["@ElementInstanceRefId"].Value;
                    }

                    openedMessage.UserId = (Guid)cmd.Parameters["@UserId"].Value;
                    openedMessage.UserName = (string)cmd.Parameters["@UserName"].Value;
                }
            }

            return openedMessage;
        }

        public void Create(int elementInstanceId)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Create_Messages", cn) { CommandType = CommandType.StoredProcedure };
                
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                cmd.Parameters.Add(new SqlParameter("@ElementInstanceId", SqlDbType.Int) { Value = elementInstanceId });

                cmd.ExecuteNonQuery();

                NotifyMessages(elementInstanceId);

                ///TODO: Hacer la notificación
            }
        }

        private void NotifyMessages(int elementInstanceId)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ActivityInstanceToNotify", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.Add(new SqlParameter("@ElementInstanceId", SqlDbType.Int) { Value = elementInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@ElementName", SqlDbType.VarChar, 255) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ElementDescription", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ElementStartDate", SqlDbType.DateTime) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@EnvironmentId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceKey", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceContent", SqlDbType.VarChar, 2000) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceKey", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceContent", SqlDbType.VarChar, 2000) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@EMails", SqlDbType.VarChar, 1000) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@NotificationSettings", SqlDbType.VarChar, 2000) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    string notificationSettings = (string)cmd.Parameters["@NotificationSettings"].Value;

                    if (notificationSettings != "{}")
                    {
                        string elementName = (string)cmd.Parameters["@ElementName"].Value;
                        string elementDescription = (string)cmd.Parameters["@ElementDescription"].Value;
                        DateTime elementStartDate = (DateTime)cmd.Parameters["@ElementStartDate"].Value;
                        Guid environmentId = (Guid)cmd.Parameters["@EnvironmentId"].Value;
                        string procedureInstanceKey = (string)cmd.Parameters["@ProcedureInstanceKey"].Value;
                        string procedureInstanceContent = (string)cmd.Parameters["@ProcedureInstanceContent"].Value;
                        string flowInstanceKey = (string)cmd.Parameters["@FlowInstanceKey"].Value;
                        string flowInstanceContent = (string)cmd.Parameters["@FlowInstanceContent"].Value;
                        string emails = (string)cmd.Parameters["@Emails"].Value;

                        JObject joProcedureInstanceContent = JObject.Parse((string)cmd.Parameters["@ProcedureInstanceContent"].Value);
                        JObject joFlowInstanceContent = JObject.Parse((string)cmd.Parameters["@FlowInstanceContent"].Value);

                        notificationSettings = notificationSettings.Replace("{{ElementName}}", elementName);
                        notificationSettings = notificationSettings.Replace("{{ElementDescription}}", elementDescription);
                        notificationSettings = notificationSettings.Replace("{{ElementStartDate}}", elementStartDate.ToString("dd/MM/yyyy hh:mm"));
                        notificationSettings = notificationSettings.Replace("{{EnvironmentId}}", environmentId.ToString());
                        notificationSettings = notificationSettings.Replace("{{ProcedureInstanceKey}}", procedureInstanceKey);
                        notificationSettings = notificationSettings.Replace("{{FlowInstanceKey}}", flowInstanceKey);
                        notificationSettings = notificationSettings.Replace("{{Emails}}", emails);

                        dynamic dynNotificationSettings = JsonConvert.DeserializeObject<ExpandoObject>(notificationSettings, new ExpandoObjectConverter());

                        IDictionary<string, object> dicGenericJson = dynNotificationSettings.MessageBody.GenericJson;

                        string jsonPath = "";
                        foreach (string key in dicGenericJson.Keys)
                        {
                            jsonPath = (string)dicGenericJson[key];
                            if (jsonPath.Contains("[["))
                            {
                                jsonPath = jsonPath.Replace("[[", "").Replace("]]", "");

                                if (jsonPath.Contains("ProcedureInstanceContent."))
                                {
                                    jsonPath = jsonPath.Replace("ProcedureInstanceContent.", "");
                                    notificationSettings = notificationSettings.Replace((string)dicGenericJson[key], (string)joProcedureInstanceContent.SelectToken(jsonPath));
                                }
                                else if (jsonPath.Contains("FlowInstanceContent."))
                                {
                                    jsonPath = jsonPath.Replace("FlowInstanceContent.", "");
                                    notificationSettings = notificationSettings.Replace((string)dicGenericJson[key], (string)joFlowInstanceContent.SelectToken(jsonPath));
                                }
                            }
                        }

                        BusCall busCall = new BusCall(Configuration);

                        busCall.SendMessage("template", notificationSettings);

                        cmd.CommandText = "EXECUTION.usp_Set_ActivityInstanceNotificationSettings";

                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(new SqlParameter("@ElementInstanceId", SqlDbType.Int) { Value = elementInstanceId });
                        cmd.Parameters.Add(new SqlParameter("@NotificationSettings", SqlDbType.VarChar, 2000) { Value = notificationSettings });

                        cmd.ExecuteNonQuery();

                    }
                }
            }
        }

        public List<Message> GetReceived()
        {
            List<Message> messages = new List<Message>();

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_MessageReceived", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    ExpandoObjectConverter expandoConverter = new ExpandoObjectConverter();
                    while (reader.Read())
                    {
                        messages.Add(new Message()
                        {
                            Id = reader.GetGuid(0),
                            ActivityName = reader.GetString(1),
                            FlowName = reader.GetString(2),
                            FlowInstanceKey = reader.GetString(3),
                            FlowInstanceContent = JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(4), expandoConverter),
                            ProcedureInstanceKey = reader.GetString(5),
                            ProcedureInstanceContent = JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(6), expandoConverter),
                            StatesFlowInstance = JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(7), expandoConverter),
                            StatesProcedureInstance = JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(8), expandoConverter),
                            Start = reader.GetDateTime(9),
                            Viewed = reader.GetBoolean(10),
                            ActivityUserGroupTypeId = reader.GetInt32(11),
                            Status = reader.GetInt32(12)
                        });
                    }
                }
            }

            return messages;
        }

        public int GetReceivedCount()
        {
            int messagesCount = 0;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_MessageReceivedCount", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                cmd.Parameters.Add(new SqlParameter("@Count", SqlDbType.Int) { Direction = ParameterDirection.Output });

                cmd.ExecuteNonQuery();

                messagesCount = (int)cmd.Parameters["@Count"].Value;
            }

            return messagesCount;
        }

        /// <summary>
        /// It allows get a collection of activities draft messages from user name
        /// </summary>
        /// <param name="userId">User name</param>
        /// <returns>List of messages</returns>
        public List<Message> GetDrafts(int? pageLimit = null, int? page = null)
        {
            List<Message> messages = new List<Message>();
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_MessageDraft", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    ExpandoObjectConverter expandoConverter = new ExpandoObjectConverter();
                    while (reader.Read())
                    {
                        messages.Add(new Message()
                        {
                            Id = reader.GetGuid(0),
                            ActivityName = reader.GetString(1),
                            FlowName = reader.GetString(2),
                            FlowInstanceKey = reader.GetString(3),
                            FlowInstanceContent = JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(4), expandoConverter),
                            StatesFlowInstance = JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(5), expandoConverter),
                            Start = reader.GetDateTime(6)
                        });
                    }
                }
            }

            return messages;
        }

        public void SetStatus()
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_MessageStatus", cn) { CommandType = CommandType.StoredProcedure };

                cmd.ExecuteNonQuery();
            }

        }
    }
}
