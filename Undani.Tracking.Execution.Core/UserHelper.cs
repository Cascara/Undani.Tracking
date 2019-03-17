using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Undani.Tracking.Execution.Core
{
    public class UserHelper : Helper
    {
        public UserHelper(IConfiguration configuration, Guid userId, string token = "") : base(configuration, userId, token) { }

        public string Get(Guid? id)
        {
            string userName;
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.sp_Get_User", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = id.HasValue ? id.Value : Guid.Empty });
                    cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.VarChar, 56) { Direction = ParameterDirection.Output });
                    cmd.ExecuteNonQuery();

                    userName = (string)cmd.Parameters["@UserName"].Value;
                }
            }

            return userName;
        }
    }
}
