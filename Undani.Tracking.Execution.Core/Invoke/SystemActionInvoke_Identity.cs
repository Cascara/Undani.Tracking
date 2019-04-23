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
        public bool Identity(Guid systemActionInstanceId, string alias, string configuration)
        {
            bool start = false;
            switch (alias)
            {
                case "CreateUser":
                    start = CreateUser(systemActionInstanceId, alias, configuration);
                    break;

                default:
                    throw new Exception("The method is not implemented");
            }

            return start;
        }

        private bool CreateUser(Guid systemActionInstanceId, string alias, string configuration)
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

            _User _user = new IdentityCall(Configuration).CreateUser(configuration, oJson);

            if (_user.SubjectId != Guid.Empty)
            {
                UserHelper userHelper = new UserHelper(Configuration, UserId, Token);

                userHelper.Create(_user.SubjectId, _user.OwnerId, _user.UserName, _user.GivenName, _user.FamilyName, _user.Email, _user.RFC, JsonConvert.SerializeObject(new { systemActionInstanceId = systemActionInstanceId }));

                start = true;
            }

            return start;
        }
    }
}
