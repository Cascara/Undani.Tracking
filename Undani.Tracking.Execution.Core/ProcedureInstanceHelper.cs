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

        public Guid Create(Guid procedureRefId, Guid? activityInstanceRefId)
        {
            int procedureInstanceId = 0;
            int flowId = 0;
            int activityInstanceId = 0;
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Create_ProcedureInstance", cn) { CommandType = CommandType.StoredProcedure })
                {

                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = UserId });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureRefId", SqlDbType.UniqueIdentifier) { Value = procedureRefId });
                    cmd.Parameters.Add(new SqlParameter("@ActivityInstanceRefId", SqlDbType.UniqueIdentifier) { Value = activityInstanceRefId.HasValue ? activityInstanceRefId.Value : Guid.Empty });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceId", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@FlowId", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ActivityInstanceId", SqlDbType.Int) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    procedureInstanceId = (int)cmd.Parameters["@ProcedureInstanceId"].Value;
                    flowId = (int)cmd.Parameters["@FlowId"].Value;
                    activityInstanceId = (int)cmd.Parameters["@ActivityInstanceId"].Value;
                }
            }

            return new FlowInstanceHelper(Configuration, UserId, Token).Create(flowId, procedureInstanceId, activityInstanceId);
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
                                ActivityInstances = new ActivityInstanceHelper(Configuration, UserId, Token).GetSummaryLog(null, (int)dr["Id"]),
                                StartDate = (DateTime)dr["SartDate"],
                                EndDate = (DateTime)dr["SartDate"],
                                EnvironmentId = (Guid)dr["EnvironmentId"]
                            };
                        }
                        else
                            throw new Exception("Could not get the activity");
                    }
                }
            }

            return procedureInstance;
        }
    }
}
