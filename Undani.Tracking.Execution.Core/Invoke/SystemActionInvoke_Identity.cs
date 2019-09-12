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
using Newtonsoft.Json;
using System.Dynamic;
using Newtonsoft.Json.Converters;
using Undani.Tracking.Execution.Core;

namespace Undani.Tracking.Core.Invoke
{
    public partial class SystemActionInvoke
    {
        public bool Identity(Guid systemActionInstanceId, string alias, string settings)
        {
            bool start = false;
            switch (alias)
            {
                case "CreateUser":
                    start = CreateUser(systemActionInstanceId, settings);
                    break;

                default:
                    throw new NotImplementedException();
            }

            return start;
        }

        private bool CreateUser(Guid systemActionInstanceId, string settings)
        {
            bool start = false;

            string json = new FormCall(Configuration).GetInstanceObject(systemActionInstanceId, Token);

            dynamic oJson = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());

            Guid ownerId;
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_OwnerSystemActionInstance", cn))
                {

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@OwnerId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    ownerId = (Guid)cmd.Parameters["@OwnerId"].Value;

                    IDictionary<string, object> dJson = oJson;
                    dJson.Add("OwnerId", ownerId);
                }
            }

            dynamic dyConfiguration = JsonConvert.DeserializeObject<ExpandoObject>(settings, new ExpandoObjectConverter());

            IDictionary<string, object> dicConfiguration = dyConfiguration;

            foreach (string key in dicConfiguration.Keys)
            {
                if (dicConfiguration[key].ToString().Contains("[["))
                {
                    settings = settings.Replace((string)dicConfiguration[key], (string)oJson.SelectToken(dicConfiguration[key].ToString().Replace("[[", "").Replace("]]", "")));
                }                
            }

            settings = settings.Replace("{{OwnerId}}", ownerId.ToString());

            _User _user = new IdentityCall(Configuration).CreateUser(settings);

            if (_user.SubjectId != Guid.Empty)
            {
                dyConfiguration = JsonConvert.DeserializeObject<ExpandoObject>(settings, new ExpandoObjectConverter());

                string reference = dyConfiguration.Reference;

                string roles = dyConfiguration.Roles;

                UserHelper userHelper = new UserHelper(Configuration, UserId, Token);

                userHelper.Create(_user.SubjectId, ownerId, reference, roles, _user.UserName, _user.GivenName, _user.FamilyName, _user.Email, JsonConvert.SerializeObject(new { systemActionInstanceId = systemActionInstanceId }));

                start = true;
            }

            SetConfiguration(systemActionInstanceId, settings);

            return start;
        }
    }
}
