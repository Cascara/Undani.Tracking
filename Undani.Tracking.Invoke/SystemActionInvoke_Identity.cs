using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Text;
using Undani.Tracking.Invoke.Resource;
using Undani.Tracking.Invoke.Infra;
using System.Data;

namespace Undani.Tracking.Invoke
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

            dynamic obj = new FormCall(Configuration).GetInstanceObject(systemActionInstanceId, Token);

            _User _user = new IdentityCall(Configuration).CreateUser(configuration, obj);

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Create_User", cn))
                {

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = _user.SubjectId });
                    cmd.Parameters.Add(new SqlParameter("@OwnerId", SqlDbType.UniqueIdentifier) { Value = obj.OwnerId });
                    cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.VarChar, 256) { Value = _user.UserName });
                    cmd.Parameters.Add(new SqlParameter("@GivenName", SqlDbType.VarChar, 100) { Value = _user.GivenName });
                    cmd.Parameters.Add(new SqlParameter("@FamilyName", SqlDbType.VarChar, 100) { Value = _user.FamilyName });
                    cmd.Parameters.Add(new SqlParameter("@EMail", SqlDbType.VarChar, 256) { Value = _user.Email });

                    cmd.ExecuteNonQuery();
                }
            }

            if (_user.SubjectId != Guid.Empty)
                start = true;

            return start;
        }
    }
}
