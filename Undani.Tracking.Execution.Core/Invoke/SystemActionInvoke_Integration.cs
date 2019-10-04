using System;
using System.Data;
using System.Data.SqlClient;
using Undani.Tracking.Execution.Core.Resource;

namespace Undani.Tracking.Core.Invoke
{
    public partial class SystemActionInvoke
    {
        public bool Integration(Guid systemActionInstanceId, string alias, string settings, bool isStrict)
        {
            bool start = false;
            switch (alias)
            {
                case "FormInstanceIntegration":
                    start = FormInstanceIntegration(systemActionInstanceId, settings, isStrict);
                    break;

                default:
                    throw new NotImplementedException();
            }

            return start;
        }

        private bool FormInstanceIntegration(Guid systemActionInstanceId, string settings, bool isStrict)
        {
            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_FormInstanceIntegration", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@FormInstanceId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    Guid systemActionInstanceIdRequest = systemActionInstanceId;
                    if (!isStrict)
                        systemActionInstanceIdRequest = Guid.Empty;

                    settings = settings.Replace("{{SystemActionInstanceId}}", systemActionInstanceIdRequest.ToString());
                    settings = settings.Replace("{{FormInstanceId}}", cmd.Parameters["@FormInstanceId"].Value.ToString());

                    BusCall busCall = new BusCall(Configuration);

                    busCall.SendMessage("integration", settings);
                }
            }

            SetConfiguration(systemActionInstanceId, settings);

            return true;
        }
    }
}
