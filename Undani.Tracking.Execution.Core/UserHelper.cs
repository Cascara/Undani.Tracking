using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Undani.Tracking.Execution.Core
{
    public class UserHelper : Helper
    {
        public UserHelper(IConfiguration configuration, Guid userId, string token = "") : base(configuration, userId, token) { }

        public void Create(Guid userId, Guid ownerId, string userName, string givenName, string familyName, string email, string rfc, string content)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Create_User", cn))
                {

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
                    cmd.Parameters.Add(new SqlParameter("@OwnerId", SqlDbType.UniqueIdentifier) { Value = ownerId });
                    cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.VarChar, 256) { Value = userName });
                    cmd.Parameters.Add(new SqlParameter("@GivenName", SqlDbType.VarChar, 100) { Value = givenName });
                    cmd.Parameters.Add(new SqlParameter("@FamilyName", SqlDbType.VarChar, 100) { Value = familyName });
                    cmd.Parameters.Add(new SqlParameter("@EMail", SqlDbType.VarChar, 256) { Value = email });
                    cmd.Parameters.Add(new SqlParameter("@RFC", SqlDbType.VarChar, 13) { Value = rfc });
                    cmd.Parameters.Add(new SqlParameter("@Content", SqlDbType.VarChar, 2000) { Value = content });

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<string> GetOwnerRoles(Guid ownerId)
        {
            List<string> roles = new List<string>();
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_UserOwnerRole", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@OwnerId", SqlDbType.UniqueIdentifier) { Value = ownerId });

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            roles.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return roles;
        }
    }
}
