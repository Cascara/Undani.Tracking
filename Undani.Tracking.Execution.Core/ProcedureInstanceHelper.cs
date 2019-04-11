﻿using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Text;

namespace Undani.Tracking.Execution.Core
{
    public class ProcedureInstanceHelper : Helper
    {
        public ProcedureInstanceHelper(IConfiguration configuration, Guid userId, string token = "") : base(configuration, userId, token) { }

        public Guid Create(Guid procedureRefId, Guid? systemActionInstanceId = null)
        {
            int procedureInstanceId = 0;
            int flowId = 0;
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Create_ProcedureInstance", cn) { CommandType = CommandType.StoredProcedure })
                {

                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureRefId", SqlDbType.UniqueIdentifier) { Value = procedureRefId });
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId ?? Guid.Empty });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceId", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowId", SqlDbType.Int) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    procedureInstanceId = (int)cmd.Parameters["@ProcedureInstanceId"].Value;
                    flowId = (int)cmd.Parameters["@FlowId"].Value;
                }
            }

            return new FlowInstanceHelper(Configuration, UserId, Token).Create(flowId, procedureInstanceId, systemActionInstanceId);
        }

        public ProcedureInstance Get(Guid procedureInstanceRefId)
        {
            ProcedureInstance procedureInstance;
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ProcedureInstance", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceRefId", SqlDbType.UniqueIdentifier) { Value = procedureInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count == 1)
                        {
                            DataRow dr = dt.Rows[0];
                            ExpandoObjectConverter expandoConverter = new ExpandoObjectConverter();
                            procedureInstance = new ProcedureInstance()
                            {
                                RefId = procedureInstanceRefId,
                                ProcedureName = (string)dr["ProcedureName"],
                                ProcedureKey = (string)dr["ProcedureKey"],
                                Key = (string)dr["Key"],
                                Content = JsonConvert.DeserializeObject<ExpandoObject>((string)dr["Content"], expandoConverter),
                                States = JsonConvert.DeserializeObject<ExpandoObject>((string)dr["States"], expandoConverter),
                                ActivityInstances = new ActivityInstanceHelper(Configuration, UserId, Token).GetLogProcedureInstance((int)dr["Id"]),
                                StartDate = (DateTime)dr["StartDate"],
                                EnvironmentId = (Guid)dr["EnvironmentId"]
                            };

                            if (dr["EndDate"] != DBNull.Value)
                                procedureInstance.EndDate = (DateTime)dr["EndDate"];
                        }
                        else
                            throw new Exception("Could not get the activity");
                    }
                }
            }

            return procedureInstance;
        }

        public List<ProcedureInstanceSummary> GetInProcess(Guid userId)
        {
            List<ProcedureInstanceSummary> procedures = new List<ProcedureInstanceSummary>();

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ProcedureInstanceInProcess", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    ExpandoObjectConverter expandoConverter = new ExpandoObjectConverter();
                    while (reader.Read())
                    {
                        procedures.Add(new ProcedureInstanceSummary()
                        {
                            RefId = reader.GetGuid(0),
                            Name = reader.GetString(1),
                            Key = reader.GetString(2),
                            Content = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(3), expandoConverter),
                            Start = reader.GetDateTime(4),
                            PrincipalState = reader.GetString(5)
                        });
                    }
                }
            }

            return procedures;
        }

        public int GetInProcessCount(Guid userId)
        {
            int inProcessCount = 0;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ProcedureInstanceInProcessCount", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                cmd.Parameters.Add(new SqlParameter("@Count", SqlDbType.Int) { Direction = ParameterDirection.Output });

                cmd.ExecuteNonQuery();

                inProcessCount = (int)cmd.Parameters["@Count"].Value;
            }

            return inProcessCount;
        }

        public List<ProcedureInstanceSummary> GetResolved(Guid userId)
        {
            List<ProcedureInstanceSummary> procedures = new List<ProcedureInstanceSummary>();

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ProcedureInstanceResolved", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    ExpandoObjectConverter expandoConverter = new ExpandoObjectConverter();
                    while (reader.Read())
                    {
                        procedures.Add(new ProcedureInstanceSummary()
                        {
                            RefId = reader.GetGuid(0),
                            Name = reader.GetString(1),
                            Key = reader.GetString(2),
                            Content = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(reader.GetString(3), expandoConverter),
                            Start = reader.GetDateTime(4),
                            End = reader.GetDateTime(5),
                            PrincipalState = reader.GetString(6)
                        });
                    }
                }
            }

            return procedures;
        }

        public List<ActivityInstanceSummary> GetLog(Guid procedureInstanceRefId)
        {
            List<ActivityInstanceSummary> activityLog = new List<ActivityInstanceSummary>();

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ProcedureInstanceLog", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceRefId", SqlDbType.UniqueIdentifier) { Value = procedureInstanceRefId });

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        activityLog.Add(new ActivityInstanceSummary()
                        {
                            RefId = reader.GetGuid(0),
                            ActivityName = reader.GetString(1),
                            UserName = reader.GetString(2),
                            Start = reader.GetDateTime(3),
                            End = reader.IsDBNull(4) ? new DateTime() : reader.GetDateTime(4),
                            Days = reader.GetString(5),
                            Hours = reader.GetString(6),
                            Reference = reader.GetString(7)
                        });
                    }
                }
            }

            return activityLog;
        }

        public List<Comment> GetComments(Guid procedureInstanceRefId)
        {
            List<Comment> comments = new List<Comment>();
            string scn = Configuration["CnDbTracking"];
            using (SqlConnection cn = new SqlConnection(scn))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ProcedureInstanceComments", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceRefId", SqlDbType.UniqueIdentifier) { Value = procedureInstanceRefId });

                using (SqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        comments.Add(new Comment()
                        {
                            Id = reader.GetGuid(0),
                            UserName = reader.GetString(1),
                            Text = reader.GetString(2),
                            Created = reader.GetDateTime(3),
                            IsMe = reader.GetBoolean(4)
                        });
                    }
                }
            }

            return comments;
        }

        public void SetComment(Guid activityInstanceRefId, string comment)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_ProcedureInstanceComment", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@ActivityInstanceRefId", SqlDbType.UniqueIdentifier) { Value = activityInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@Comment", SqlDbType.VarChar, 255) { Value = comment });
                    cmd.ExecuteNonQuery();
                }
            }
        }

    }
}
