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
    public static class ProcedureInstanceHelper
    {
        public static ProcedureInstance Get(Guid procedureInstanceRefId, Guid userId)
        {
            ProcedureInstance procedureInstance;
            using (SqlConnection cn = new SqlConnection(Configuration.GetValue("ConnectionString:Tracking")))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_ProcedureInstance", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceRefId", SqlDbType.UniqueIdentifier) { Value = procedureInstanceRefId });
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });

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
                                ActivityInstances = ActivityInstanceHelper.GetSummaryLog(userId, null, (int)dr["Id"]),
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
