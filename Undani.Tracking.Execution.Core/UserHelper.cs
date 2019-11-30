using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;

namespace Undani.Tracking.Execution.Core
{
    public class UserHelper : Helper
    {
        public UserHelper(IConfiguration configuration, Guid userId, string token = "") : base(configuration, userId, token) { }

        public void Create(Guid userId, Guid ownerId, string reference, string roles, string userName, string givenName, string familyName, string email, string content)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Create_User", cn))
                {

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
                    cmd.Parameters.Add(new SqlParameter("@OwnerId", SqlDbType.UniqueIdentifier) { Value = ownerId });
                    cmd.Parameters.Add(new SqlParameter("@Reference", SqlDbType.VarChar, 100) { Value = reference });
                    cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.VarChar, 256) { Value = userName });
                    cmd.Parameters.Add(new SqlParameter("@GivenName", SqlDbType.VarChar, 100) { Value = givenName });
                    cmd.Parameters.Add(new SqlParameter("@FamilyName", SqlDbType.VarChar, 100) { Value = familyName });
                    cmd.Parameters.Add(new SqlParameter("@EMail", SqlDbType.VarChar, 256) { Value = email });
                    cmd.Parameters.Add(new SqlParameter("@Content", SqlDbType.VarChar, 2000) { Value = content });

                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "EXECUTION.usp_Set_UserOwnerRole";
                     
                    cmd.Parameters.RemoveAt("@Reference");
                    cmd.Parameters.RemoveAt("@UserName");
                    cmd.Parameters.RemoveAt("@GivenName");
                    cmd.Parameters.RemoveAt("@FamilyName");
                    cmd.Parameters.RemoveAt("@EMail");
                    cmd.Parameters.RemoveAt("@Content");

                    cmd.Parameters.Add(new SqlParameter("@Role", SqlDbType.VarChar, 150));

                    string[] aRoles = roles.Split(',');

                    foreach (string role in aRoles)
                    {
                        cmd.Parameters["@Role"].Value = role;

                        cmd.ExecuteNonQuery();
                    }
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

        public User Get(Guid ownerId)
        {
            User user = new User();
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_User", cn))
                {

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@OwnerId", SqlDbType.UniqueIdentifier) { Value = ownerId });
                    cmd.Parameters.Add(new SqlParameter("@Reference", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.VarChar, 256) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.VarChar, 256) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@GivenName", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FamilyName", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@EMail", SqlDbType.VarChar, 256) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@Content", SqlDbType.VarChar, 2000) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    user.Id = UserId;
                    user.OwnerId = ownerId;
                    user.Reference = (string)cmd.Parameters["@Reference"].Value;
                    user.UserName = (string)cmd.Parameters["@UserName"].Value;
                    user.Name = (string)cmd.Parameters["@Name"].Value;
                    user.GivenName = (string)cmd.Parameters["@GivenName"].Value;
                    user.FamilyName = (string)cmd.Parameters["@FamilyName"].Value;
                    user.EMail = (string)cmd.Parameters["@EMail"].Value;
                    user.Content = JsonConvert.DeserializeObject<ExpandoObject>((string)cmd.Parameters["@Content"].Value, new ExpandoObjectConverter());
                }
            }

            return user;
        }

        public List<UserSummary> Get(string role)
        {
            List<UserSummary> userSummaries = new List<UserSummary>();
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_Users", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@Role", SqlDbType.VarChar, 50) { Value = role });

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userSummaries.Add(new UserSummary() { 
                                Id = reader.GetGuid(0),
                                GivenName = reader.GetString(1),
                                FamilyName = reader.GetString(2),
                                EMail = reader.GetString(3)
                            });
                        }
                    }
                }
            }

            return userSummaries;
        }

        public void SetContent (Guid userId, string content)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_UserContent", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
                    cmd.Parameters.Add(new SqlParameter("@Content", SqlDbType.VarChar, 2000) { Value = content });

                    cmd.ExecuteNonQuery();                    
                }
            }
        }
    }
}
