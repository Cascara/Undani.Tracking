using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Undani.Tracking.Invoke.Resource;
using Undani.Tracking.Invoke.Resource.Infra;

namespace Undani.Tracking.Invoke
{
    public static partial class SystemActionInvoke
    {
        public static bool Identity(Guid systemActionInstanceId, string alias, string configuration)
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

        private static bool CreateUser(Guid systemActionInstanceId, string alias, string configuration)
        {
            bool start = false;

            dynamic obj = FormRequest.GetInstanceObject(systemActionInstanceId);

            _User _user = IdentityRequest.CreateUser(configuration, obj);

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
                    openedMessage.UserName = (string)cmd.Parameters["@UserName"].Value;
                }
            }

            if (_user.SubjectId != Guid.Empty)
                start = true;

            return start;
        }
    }
}
