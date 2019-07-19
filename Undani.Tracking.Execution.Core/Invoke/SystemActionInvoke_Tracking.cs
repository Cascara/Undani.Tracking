using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Text;
using Undani.Tracking.Execution.Core.Invoke.Resource;
using Undani.Tracking.Execution.Core.Invoke.Infra;
using System.Data;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Undani.Tracking.Execution.Core;

namespace Undani.Tracking.Core.Invoke
{
    public partial class SystemActionInvoke
    {
        public bool Tracking(Guid systemActionInstanceId, string alias, string configuration)
        {
            bool start = false;
            switch (alias)
            {
                case "ProcedureInstanceStartDate":
                    start = ProcedureInstanceStartDate(systemActionInstanceId);
                    break;

                case "ProcedureInstanceEndDate":
                    start = ProcedureInstanceEndDate(systemActionInstanceId, configuration);
                    break;

                case "ProcedureInstanceUnique":
                    start = ProcedureInstanceUnique(systemActionInstanceId, configuration);
                    break;

                case "ProcedureInstanceFormDocument":
                    start = ProcedureInstanceFormDocument(systemActionInstanceId, configuration);
                    break;

                case "ProcedureInstanceFormDocumentToPDF":
                    start = ProcedureInstanceFormDocumentToPDF(systemActionInstanceId, configuration);
                    break;

                case "FlowInstanceStartDate":
                    start = FlowInstanceStartDate(systemActionInstanceId);
                    break;

                case "CreateFlowInstance":
                    start = CreateFlowInstance(systemActionInstanceId, configuration);
                    break;

                default:
                    throw new Exception("The method is not implemented");
            }

            return start;
        }

        private bool ProcedureInstanceStartDate(Guid systemActionInstanceId)
        {
            bool start = false;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_ProcedureInstanceStartDate", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });

                    cmd.ExecuteNonQuery();
                    start = true;
                }
            }               

            return start;
        }

        private bool ProcedureInstanceEndDate(Guid systemActionInstanceId, string configuration)
        {
            bool start = false;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_ProcedureInstanceEndDate", cn))
                {
                    JObject oJson = JObject.Parse(configuration);

                    JToken token = JToken.FromObject(oJson);

                    string key = token["Key"].ToString();

                    string[] states = token["States"].ToString().Split(',');

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@Key", SqlDbType.VarChar, 50) { Value = key });
                    cmd.Parameters.Add(new SqlParameter("@State", SqlDbType.VarChar, 50));

                    foreach (string state in states)
                    {
                        cmd.Parameters["@State"].Value = state;
                        cmd.ExecuteNonQuery();
                    }

                    start = true;
                }

                return start;
            }
        }

        private bool ProcedureInstanceUnique(Guid systemActionInstanceId, string configuration)
        {
            bool start = false;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_ProcedureInstanceUnique", cn))
                {
                    JObject oJson = JObject.Parse(configuration);

                    JToken token = JToken.FromObject(oJson);

                    string key = token["Key"].ToString();

                    string[] states = token["States"].ToString().Split(',');

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@Key", SqlDbType.VarChar, 50) { Value = key });
                    cmd.Parameters.Add(new SqlParameter("@State", SqlDbType.VarChar, 50));

                    foreach (string state in states)
                    {
                        cmd.Parameters["@State"].Value = state;
                        cmd.ExecuteNonQuery();
                    }

                    start = true;
                }

                return start;
            }
        }

        private bool ProcedureInstanceFormDocument(Guid systemActionInstanceId, string configuration)
        {
            bool start = false;

            dynamic jsonConfiguration = JsonConvert.DeserializeObject<ExpandoObject>(configuration, new ExpandoObjectConverter());

            JObject oJson = JObject.Parse(new FormCall(Configuration).GetInstanceObject(systemActionInstanceId, Token));

            JToken jToken = oJson.SelectToken(jsonConfiguration.Path);

            string documents = "";

            if (jToken.Type != JTokenType.Array)
            {
                documents = "[" + JsonConvert.SerializeObject(jToken) + "]";
            }
            else
            {
                documents = JsonConvert.SerializeObject(jToken);               
            }

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_ProcedureInstanceFormDocument", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@Key", SqlDbType.VarChar, 50) { Value = jsonConfiguration.Key });
                    cmd.Parameters.Add(new SqlParameter("@Document", SqlDbType.VarChar, 500) { Value = documents.Replace(".docx", ".pdf").Replace(".DOCX", ".pdf") });

                    cmd.ExecuteNonQuery();

                    start = true;
                }

                return start;
            }
        }

        private bool ProcedureInstanceFormDocumentToPDF(Guid systemActionInstanceId, string configuration)
        {
            bool start = false;

            dynamic jsonConfiguration = JsonConvert.DeserializeObject<ExpandoObject>(configuration, new ExpandoObjectConverter());

            JObject oJson = JObject.Parse(new FormCall(Configuration).GetInstanceObject(systemActionInstanceId, Token));

            JToken jToken = oJson.SelectToken(jsonConfiguration.Path);

            string documents = "";
            List<string> documentsToConvert = new List<string>();

            if (jToken.Type != JTokenType.Array)
            {
                documents = "[" + JsonConvert.SerializeObject(jToken) + "]";
                documentsToConvert.Add((string)jToken["SystemName"]);
            }
            else
            {
                documents = JsonConvert.SerializeObject(jToken);
                string document = "";
                foreach (JToken item in jToken)
                {
                    document = (string)item["SystemName"];
                    if ((document.Contains(".docx") || document.Contains(".DOCX")) && (bool)item["ToPDF"])
                    {
                        documentsToConvert.Add((string)item["SystemName"]);
                    }
                }
            }

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_ProcedureInstanceFormDocumentToPDF", cn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@Key", SqlDbType.VarChar, 50) { Value = jsonConfiguration.Key });
                    cmd.Parameters.Add(new SqlParameter("@Document", SqlDbType.VarChar, 500) { Value = documents.Replace(".docx", ".pdf").Replace(".DOCX", ".pdf") });
                    cmd.Parameters.Add(new SqlParameter("@OwnerId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    start = new BoxCall(Configuration).DocxToPDF(systemActionInstanceId, (Guid)cmd.Parameters["@OwnerId"].Value, documentsToConvert);

                }

                return start;
            }
        }

        private bool FlowInstanceStartDate(Guid systemActionInstanceId)
        {
            bool start = false;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_FlowInstanceStartDate", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });

                    cmd.ExecuteNonQuery();
                    start = true;
                }
            }

            return start;
        }

        private bool CreateFlowInstance(Guid systemActionInstanceId, string configuration)
        {
            bool start = false;            

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_CreateFlowInstance", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@Key", SqlDbType.VarChar, 50));
                    cmd.Parameters.Add(new SqlParameter("@State", SqlDbType.VarChar, 50));
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceId", SqlDbType.Int) { Direction = ParameterDirection.Output });

                    List<_CreateFlow> createFlows = JsonConvert.DeserializeObject<List<_CreateFlow>>(configuration);

                    FlowInstanceHelper flowInstanceHelper = new FlowInstanceHelper(Configuration, UserId);
                    int procedureInstanceId;
                    foreach (_CreateFlow createFlow in createFlows)
                    {
                        cmd.Parameters["@Key"].Value = createFlow.Key;
                        cmd.Parameters["@State"].Value = createFlow.State;

                        cmd.ExecuteNonQuery();

                        procedureInstanceId = (int)cmd.Parameters["@ProcedureInstanceId"].Value;

                        if (procedureInstanceId > 0)
                        {
                            flowInstanceHelper.Create(createFlow.FlowId, procedureInstanceId, systemActionInstanceId, createFlow.Version);
                        }                        
                    }
                    
                    start = true;
                }
            }

            return start;
        }
    }
}
