using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Text;

namespace Undani.Tracking.Execution.Core
{
    public class FormInstanceHelper : Helper
    {
        public FormInstanceHelper(IConfiguration configuration, Guid userId, string token = "") : base(configuration, userId, token) { }

        public void Create(Guid environmentId, int flowInstanceId, int activityInstanceId, int activityTypeId, string activityGetFormInstanceKey, Guid formId, int formVersionId, bool formReadOnly)
        {
            Create(environmentId, flowInstanceId, activityInstanceId, activityTypeId, activityGetFormInstanceKey, formId, formVersionId, Guid.Empty, formReadOnly);
        }

        public void Create(Guid environmentId, int flowInstanceId, int activityInstanceId, int activityTypeId, string activityGetFormInstanceKey, Guid formId, int formVersionId, Guid formParentInstanceId, bool formReadOnly)
        {
            Guid formInstanceId = Guid.Empty;
            if (activityTypeId == 1)
            {
                SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]);
                cn.Open();

                if (activityGetFormInstanceKey == "")
                {
                    string url = "";

                    if (formId != Guid.Empty)
                        url += "&formId=" + formId.ToString();
                    else
                        throw new Exception("It was not possible to create the instance of the form for human activity");

                    if (formVersionId > 0)
                        url += "&version=" + formVersionId.ToString();

                    if (formParentInstanceId != Guid.Empty)
                        url += "&parentInstanceId=" + formParentInstanceId.ToString();

                    if (formReadOnly)
                        url += "&readOnly=true";

                    if (url != string.Empty)
                    {
                        url = Configuration["ApiForm"] + "/Execution/Form/createInstance?environmentId=" + environmentId.ToString() + url;

                        HttpResponseMessage response = null;
                        using (var client = new HttpClient())
                        {
                            StringContent content = new StringContent("", Encoding.UTF8, "text/plain");
                            response = client.PostAsync(url, content).Result;
                            formInstanceId = Guid.Parse(response.Content.ReadAsStringAsync().Result.Replace("\"", ""));
                        }
                    }
                }
                else
                {
                    using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Get_FormInstance", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter("@FlowInstanceId", SqlDbType.Int) { Value = flowInstanceId });
                        cmd.Parameters.Add(new SqlParameter("@Key", SqlDbType.VarChar, 50) { Value = activityGetFormInstanceKey });
                        cmd.Parameters.Add(new SqlParameter("@FormInstanceId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });

                        cmd.ExecuteNonQuery();

                        if (cmd.Parameters["@FormInstanceId"].Value.ToString() == "")
                            throw new Exception("Something is wrong with the global form instance");

                        formInstanceId = (Guid)cmd.Parameters["@FormInstanceId"].Value;
                    }
                }

                if (formInstanceId == Guid.Empty)
                    throw new Exception("It was not possible to create the instance of the form for human activity");

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_ActivityInstanceFormInstanceId", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@ActivityInstanceId", SqlDbType.Int) { Value = activityInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@FormInstanceId", SqlDbType.UniqueIdentifier) { Value = formInstanceId });

                    cmd.ExecuteNonQuery();
                }

                cn.Close();
            }
        }


    }
}
