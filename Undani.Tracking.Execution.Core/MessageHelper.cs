using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;

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

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Create_Messages", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                cmd.Parameters.Add(new SqlParameter("@ElementInstanceId", SqlDbType.Int) { Value = elementInstanceId });

                cmd.ExecuteNonQuery();
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
                            FlowInstanceContent = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(4), expandoConverter),
                            ProcedureInstanceKey = reader.GetString(5),
                            ProcedureInstanceContent = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(6), expandoConverter),
                            StatesFlowInstance = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(7), expandoConverter),
                            StatesProcedureInstance = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(8), expandoConverter),
                            Start = reader.GetDateTime(9),
                            Viewed = reader.GetBoolean(10),
                            ActivityUserGroupTypeId = reader.GetInt32(11)
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
                            FlowInstanceContent = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(4), expandoConverter),
                            StatesFlowInstance = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(5), expandoConverter),
                            Start = reader.GetDateTime(6)
                        });
                    }
                }
            }

            return messages;
        }
    }
}
