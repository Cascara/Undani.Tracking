using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Text;
using Undani.Tracking.Core.Invoke.Resource;
using Undani.Tracking.Core.Invoke.Infra;
using System.Data;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Undani.Tracking.Core.Invoke
{
    public partial class SystemActionInvoke
    {
        public bool Response(Guid systemActionInstanceId, string alias, string configuration)
        {
            bool start = false;
            switch (alias)
            {
                case "FlowInstanceResponse":
                    start = FlowInstanceResponse(systemActionInstanceId, configuration);
                    break;

                default:
                    throw new Exception("The method is not implemented");
            }

            return start;
        }

        private bool FlowInstanceResponse(Guid systemActionInstanceId, string configuration)
        {
            bool start = false;

            string json = new FormCall(Configuration).GetInstanceObject(systemActionInstanceId, Token);

            JObject oJson = JObject.Parse(json);

            JToken token = oJson.SelectToken(configuration);

            List<string> documents = new List<string>();
            if (token.GetType() == typeof(JArray))
            {
                foreach (var item in token)
                {
                    documents.Add(item["SystemName"].ToString());
                }
            }
            else
            {

                documents.Add(token["SystemName"].ToString());
            }

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_FlowInstanceResponse", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@Response", SqlDbType.VarChar, 2000) { Value = JsonConvert.SerializeObject(documents) });

                    cmd.ExecuteNonQuery();
                    start = true;
                }
            }               

            return start;
        }

    }
}
