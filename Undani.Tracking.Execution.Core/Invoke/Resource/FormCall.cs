using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Text;
namespace Undani.Tracking.Execution.Core.Invoke.Resource
{
    internal class FormCall : Call
    {
        public FormCall(IConfiguration configuration) : base(configuration) { }

        public string GetInstanceObject(Guid systemActionInstanceId, string token)
        {
            Guid formInstanceId;
            string json = "";
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_FormInstanceObject", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@FormInstanceId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    formInstanceId = (Guid)cmd.Parameters["@FormInstanceId"].Value;
                }
            }

            string url = Configuration["ApiForm"] + "/Execution/GetJsonInstance?instanceId=" + formInstanceId;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                HttpResponseMessage response = client.GetAsync(url).Result;

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception("There was an error when trying to consume the resource apiform");

                json = response.Content.ReadAsStringAsync().Result;

            }

            return json;
        }
    }
}
