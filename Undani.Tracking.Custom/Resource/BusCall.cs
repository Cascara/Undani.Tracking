using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Undani.Tracking.Custom.Resource
{
    public class BusCall
    {
        public void SendMessage(Guid systemActionInstanceId, string cnDbTracking, string cnSrvBus, string queueName, string message)
        {
            using (SqlConnection cn = new SqlConnection(cnDbTracking))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SystemActionInstanceSettings", cn))
                {

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@Settings", SqlDbType.VarChar, -1) { Value = message });

                    cmd.ExecuteNonQuery();
                }
            }

            var queueClient = ClientBus.Bus.Connect(cnSrvBus, queueName);

            queueClient.Send(JObject.Parse(message));


        }
    }
}
