using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Text;

namespace Undani.Tracking.Execution.Core
{
    public static class MessageHelper
    {
        public static OpenedMessage GetOpen(Guid userId, Guid messageId)
        {

            OpenedMessage openedMessage = new OpenedMessage();
            using (SqlConnection cn = new SqlConnection(Configuration.GetValue("ConnectionString:Tracking")))
            {
                cn.Open();
                
                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_MessageOpen", cn))
                {

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId, Direction = ParameterDirection.InputOutput });
                    cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@MessageId", SqlDbType.UniqueIdentifier) { Value = messageId });
                    cmd.Parameters.Add(new SqlParameter("@ActivityInstanceRefId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();
                    if (cmd.Parameters["@ActivityInstanceRefId"].Value.ToString() != "")
                    {
                        openedMessage.ActivityIntanceRefId = (Guid)cmd.Parameters["@ActivityInstanceRefId"].Value;
                    }

                    openedMessage.UserId = (Guid)cmd.Parameters["@UserId"].Value;
                }
            }

            return openedMessage;
        }

        public static void Create(Guid userId, int activityInstanceId)
        {
            using (SqlConnection cn = new SqlConnection(Configuration.GetValue("ConnectionString:Tracking")))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Create_Messages", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
                cmd.Parameters.Add(new SqlParameter("@ActivityInstanceId", SqlDbType.Int) { Value = activityInstanceId });

                cmd.ExecuteNonQuery();
            }
        }

        public static List<Message> GetReceived(Guid userId)
        {
            List<Message> messages = new List<Message>();

            using (SqlConnection cn = new SqlConnection(Configuration.GetValue("ConnectionString:Tracking")))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_MessageReceived", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });

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
                            FlowInstanceContent = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(4), expandoConverter),
                            States = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(5), expandoConverter),
                            Start = reader.GetDateTime(6),
                            Viewed = reader.GetBoolean(7),
                            ActivityUserGroupTypeId = reader.GetInt32(8)
                        });
                    }
                }
            }

            return messages;
        }

        /// <summary>
        /// It allows get a collection of activities draft messages from user name
        /// </summary>
        /// <param name="userId">User name</param>
        /// <returns>List of messages</returns>
        public static List<Message> GetDrafts(Guid userId)
        {
            List<Message> messages = new List<Message>();
            using (SqlConnection cn = new SqlConnection(Configuration.GetValue("ConnectionString:Tracking")))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_MessageDraft", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });

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
                            FlowInstanceContent = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(4), expandoConverter),
                            States = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(5), expandoConverter),
                            Start = reader.GetDateTime(6)
                        });
                    }
                }
            }

            return messages;
        }
    }
}
