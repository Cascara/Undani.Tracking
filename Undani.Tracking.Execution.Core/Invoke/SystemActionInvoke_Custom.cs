using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Text;
using Undani.Tracking.Execution.Core.Invoke.Resource;
using Undani.Tracking.Execution.Core.Invoke.Infra;
using System.Data;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Undani.Tracking.Core.Invoke
{
    public partial class SystemActionInvoke
    {
        public bool Custom(Guid systemActionInstanceId, string alias, string configuration)
        {
            String ownerKey = String.Empty;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_Custom", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@OwnerId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    Crc32 crc32 = new Crc32();

                    foreach (byte b in crc32.ComputeHash(Encoding.ASCII.GetBytes(cmd.Parameters["@OwnerId"].Value.ToString().ToLower()))) ownerKey += b.ToString("x2").ToLower();

                    ownerKey = ownerKey.ToUpper();
                }
            }

            bool start = false;

            using (var client = new HttpClient())
            {
                string url = Configuration["ApiCustom_" + ownerKey] + "/Custom/SystemAction/Invoke?systemActionInstanceId=" + systemActionInstanceId.ToString() + "&alias=" + alias + "&configuration=" + configuration;

                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Token);

                HttpResponseMessage response = client.GetAsync(url).Result;

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception("There was an error when trying to invoke a custom system action");

                start = bool.Parse(response.Content.ReadAsStringAsync().Result);
            }

            return start;
        }

    }
}
