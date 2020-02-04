using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Dynamic;
using Undani.Tracking.Execution.Core;
using Undani.Tracking.Execution.Core.Invoke.Infra;
using Undani.Tracking.Execution.Core.Resource;

namespace Undani.Tracking.Core.Invoke
{
    public partial class SystemActionInvoke
    {
        public bool Tracking(Guid systemActionInstanceId, string alias, string settings, bool isStrict)
        {
            bool start = false;
            switch (alias)
            {
                case "ProcedureInstanceStartDate":
                    start = ProcedureInstanceStartDate(systemActionInstanceId, settings);
                    break;

                case "ProcedureInstanceEndDate":
                    start = ProcedureInstanceEndDate(systemActionInstanceId, settings);
                    break;

                case "ProcedureInstanceUnique":
                    start = ProcedureInstanceUnique(systemActionInstanceId, settings);
                    break;

                case "ProcedureInstanceFormDocument":
                    start = ProcedureInstanceFormDocument(systemActionInstanceId, settings);
                    break;

                case "ProcedureInstanceFormDocumentToPDF":
                    start = ProcedureInstanceFormDocumentToPDF(systemActionInstanceId, settings, isStrict);
                    break;

                case "FlowInstanceStartDate":
                    start = FlowInstanceStartDate(systemActionInstanceId, settings);
                    break;

                case "CreateFlowInstance":
                    start = CreateFlowInstance(systemActionInstanceId, settings);
                    break;

                case "EvaluateDailyActionInstancePause":
                    start = EvaluateDailyActionInstancePause(systemActionInstanceId);
                    break;

                case "EvaluateDailyProcedureInstancePause":
                    start = EvaluateDailyProcedureInstancePause(systemActionInstanceId);
                    break;

                case "ProcedureInstanceContent":
                    start = ProcedureInstanceContent(systemActionInstanceId, settings);
                    break;

                case "FlowInstanceContent":
                    start = FlowInstanceContent(systemActionInstanceId, settings);
                    break;

                case "CopyFormInstance":
                    start = CopyFormInstance(systemActionInstanceId, settings);
                    break;

                default:
                    throw new NotImplementedException();
            }

            return start;
        }

        private bool ProcedureInstanceStartDate(Guid systemActionInstanceId, string settings)
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

            SetConfiguration(systemActionInstanceId, settings);

            return start;
        }

        private bool ProcedureInstanceEndDate(Guid systemActionInstanceId, string settings)
        {
            bool start = false;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_ProcedureInstanceEndDate", cn))
                {
                    JObject oJson = JObject.Parse(settings);

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
            }

            SetConfiguration(systemActionInstanceId, settings);

            return start;
        }

        private bool ProcedureInstanceUnique(Guid systemActionInstanceId, string settings)
        {
            bool start = false;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_ProcedureInstanceUnique", cn))
                {
                    JObject oJson = JObject.Parse(settings);

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

                SetConfiguration(systemActionInstanceId, settings);

                return start;
            }
        }

        private bool ProcedureInstanceFormDocument(Guid systemActionInstanceId, string settings)
        {
            bool start = false;

            dynamic dySettings = JsonConvert.DeserializeObject<ExpandoObject>(settings, new ExpandoObjectConverter());

            JObject oJson = JObject.Parse(new FormCall(Configuration).GetInstanceObject(systemActionInstanceId, Token));

            JToken jToken = oJson.SelectToken(dySettings.Path);

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
                    cmd.Parameters.Add(new SqlParameter("@Key", SqlDbType.VarChar, 50) { Value = dySettings.Key });
                    cmd.Parameters.Add(new SqlParameter("@Document", SqlDbType.VarChar, 500) { Value = documents.Replace(".docx", ".pdf").Replace(".DOCX", ".pdf") });

                    cmd.ExecuteNonQuery();

                    start = true;
                }
            }

            SetConfiguration(systemActionInstanceId, settings);

            return start;
        }

        private bool ProcedureInstanceFormDocumentToPDF(Guid systemActionInstanceId, string settings, bool isStrict)
        {
            ExpandoObjectConverter expandoObjectConverter = new ExpandoObjectConverter();

            dynamic dySettings = JsonConvert.DeserializeObject<ExpandoObject>(settings, expandoObjectConverter);

            JObject oJson = JObject.Parse(new FormCall(Configuration).GetInstanceObject(systemActionInstanceId, Token));

            JToken jToken = oJson.SelectToken(dySettings.Path);

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
                    cmd.Parameters.Add(new SqlParameter("@Key", SqlDbType.VarChar, 50) { Value = dySettings.Key });
                    cmd.Parameters.Add(new SqlParameter("@Document", SqlDbType.VarChar, 500) { Value = documents.Replace(".docx", ".pdf").Replace(".DOCX", ".pdf") });
                    cmd.Parameters.Add(new SqlParameter("@OwnerId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    Guid systemActionInstanceIdRequest = systemActionInstanceId;
                    if (!isStrict)
                        systemActionInstanceIdRequest = Guid.Empty;

                    settings = settings.Replace("{{SystemActionInstanceId}}", systemActionInstanceIdRequest.ToString());

                    Guid ownerId = (Guid)cmd.Parameters["@OwnerId"].Value;
                    settings = settings.Replace("{{OwnerId}}", ownerId.ToString());

                    dySettings = JsonConvert.DeserializeObject<ExpandoObject>(settings, expandoObjectConverter);

                    IDictionary<string, object> dicMessageBody = dySettings.Converter.MessageBody;

                    foreach (string key in dicMessageBody.Keys)
                    {
                        if ((string)dicMessageBody[key] == "{{DocumentsToConvert}}")
                        {
                            dicMessageBody[key] = documentsToConvert;
                        }
                    }

                    BusCall busCall = new BusCall(Configuration);

                    busCall.SendMessage("docx2pdf", JsonConvert.SerializeObject(dySettings.Converter));
                }
            }

            SetConfiguration(systemActionInstanceId, JsonConvert.SerializeObject(dySettings));

            return true;
        }

        private bool FlowInstanceStartDate(Guid systemActionInstanceId, string settings)
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

            SetConfiguration(systemActionInstanceId, settings);

            return start;
        }

        private bool CreateFlowInstance(Guid systemActionInstanceId, string settings)
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

                    List<_CreateFlow> createFlows = JsonConvert.DeserializeObject<List<_CreateFlow>>(settings);

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

            SetConfiguration(systemActionInstanceId, settings);

            return start;
        }

        private bool EvaluateDailyActionInstancePause(Guid systemActionInstanceId)
        {
            bool start = false;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_ActionInstancePause", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@Start", SqlDbType.Bit) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    start = (bool)cmd.Parameters["@Start"].Value;
                }
            }

            return start;
        }

        private bool EvaluateDailyProcedureInstancePause(Guid systemActionInstanceId)
        {
            bool start = false;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_ProcedureInstancePause", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@Start", SqlDbType.Bit) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    start = (bool)cmd.Parameters["@Start"].Value;
                }
            }

            return start;
        }

        private bool ProcedureInstanceContent(Guid systemActionInstanceId, string settings)
        {
            string jsonFormInstance = new FormCall(Configuration).GetInstanceObject(systemActionInstanceId, Token);

            JObject joFormInstance = JObject.Parse(jsonFormInstance);

            JObject joSettings = JObject.Parse(settings);

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_ProcedureInstanceContent", cn) { CommandType = CommandType.StoredProcedure })
                {
                    
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceId", SqlDbType.Int) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    int procedureInstanceId = (int)cmd.Parameters["@ProcedureInstanceId"].Value;

                    cmd.CommandText = "EXECUTION.usp_Set_ProcedureInstanceContentProperty";

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceId", SqlDbType.Int) { Value = procedureInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@PropertyName", SqlDbType.VarChar, 50));
                    cmd.Parameters.Add(new SqlParameter("@Value", SqlDbType.VarChar, 1000));
                    cmd.Parameters.Add(new SqlParameter("@Type", SqlDbType.VarChar, 20));

                    dynamic dynSettings = JsonConvert.DeserializeObject<ExpandoObject>(settings, new ExpandoObjectConverter());

                    IDictionary<string, object> dicSettings = dynSettings;

                    string jsonPath = "";
                    JToken jToken;
                    string value = "";
                    foreach (string key in dicSettings.Keys)
                    {
                        if (dicSettings[key] != null)
                        {
                            jsonPath = "";

                            if (dicSettings[key].GetType().Name == "String")
                            {
                                jsonPath = (string)dicSettings[key];
                            }

                            if (jsonPath.Contains("[["))
                            {
                                jsonPath = jsonPath.Replace("[[", "").Replace("]]", "");

                                jToken = joFormInstance.SelectToken(jsonPath);

                                settings = settings.Replace((string)dicSettings[key], value);
                            }
                            else
                            {
                                jToken = joSettings.SelectToken(key);
                            }

                            if (jToken.Type == JTokenType.Object || jToken.Type == JTokenType.Array)
                            {
                                value = JsonConvert.SerializeObject(jToken);
                                cmd.Parameters["@Type"].Value = "Object";
                            }
                            else if (jToken.Type == JTokenType.Integer)
                            {
                                value = jToken.ToString();
                                cmd.Parameters["@Type"].Value = "Integer";
                            }
                            else if (jToken.Type == JTokenType.Float)
                            {
                                value = jToken.ToString();
                                cmd.Parameters["@Type"].Value = "Decimal";
                            }
                            else if (jToken.Type == JTokenType.Boolean)
                            {
                                value = jToken.ToString();
                                cmd.Parameters["@Type"].Value = "Boolean";
                            }
                            else
                            {
                                value = jToken.ToString();
                                cmd.Parameters["@Type"].Value = "String";
                            }
                        }
                        else
                        {
                            value = "";
                            cmd.Parameters["@Type"].Value = "Null";
                        }

                        cmd.Parameters["@PropertyName"].Value = key;                        

                        cmd.Parameters["@Value"].Value = value;

                        cmd.ExecuteNonQuery();                        
                    }

                    SetConfiguration(systemActionInstanceId, settings);
                }
            }

            return true;
        }

        private bool FlowInstanceContent(Guid systemActionInstanceId, string settings)
        {
            string jsonFormInstance = new FormCall(Configuration).GetInstanceObject(systemActionInstanceId, Token);

            JObject joFormInstance = JObject.Parse(jsonFormInstance);

            JObject joSettings = JObject.Parse(settings);

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_FlowInstanceContent", cn) { CommandType = CommandType.StoredProcedure })
                {

                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceId", SqlDbType.Int) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    int flowInstanceId = (int)cmd.Parameters["@FlowInstanceId"].Value;

                    cmd.CommandText = "EXECUTION.usp_Set_FlowInstanceContentProperty";

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SqlParameter("@FlowInstanceId", SqlDbType.Int) { Value = flowInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@PropertyName", SqlDbType.VarChar, 50));
                    cmd.Parameters.Add(new SqlParameter("@Value", SqlDbType.VarChar, 1000));
                    cmd.Parameters.Add(new SqlParameter("@Type", SqlDbType.VarChar, 20));                    

                    dynamic dynSettings = JsonConvert.DeserializeObject<ExpandoObject>(settings, new ExpandoObjectConverter());

                    IDictionary<string, object> dicSettings = dynSettings;

                    string jsonPath = "";
                    JToken jToken;
                    string value = "";
                    foreach (string key in dicSettings.Keys)
                    {
                        if (dicSettings[key] != null)
                        {
                            jsonPath = "";

                            if (dicSettings[key].GetType().Name == "String")
                            {
                                jsonPath = (string)dicSettings[key];
                            }

                            if (jsonPath.Contains("[["))
                            {
                                jsonPath = jsonPath.Replace("[[", "").Replace("]]", "");

                                jToken = joFormInstance.SelectToken(jsonPath);

                                settings = settings.Replace((string)dicSettings[key], value);
                            }
                            else
                            {
                                jToken = joSettings.SelectToken(key);
                            }

                            if (jToken.Type == JTokenType.Object || jToken.Type == JTokenType.Array)
                            {
                                value = JsonConvert.SerializeObject(jToken);
                                cmd.Parameters["@Type"].Value = "Object";
                            }
                            else if (jToken.Type == JTokenType.Integer)
                            {
                                value = jToken.ToString();
                                cmd.Parameters["@Type"].Value = "Integer";
                            }
                            else if (jToken.Type == JTokenType.Float)
                            {
                                value = jToken.ToString();
                                cmd.Parameters["@Type"].Value = "Decimal";
                            }
                            else if (jToken.Type == JTokenType.Boolean)
                            {
                                value = jToken.ToString();
                                cmd.Parameters["@Type"].Value = "Boolean";
                            }
                            else
                            {
                                value = jToken.ToString();
                                cmd.Parameters["@Type"].Value = "String";
                            }
                        }
                        else
                        {
                            value = "";
                            cmd.Parameters["@Type"].Value = "Null";
                        }

                        cmd.Parameters["@PropertyName"].Value = key;

                        cmd.Parameters["@Value"].Value = value;

                        cmd.ExecuteNonQuery();
                    }

                    SetConfiguration(systemActionInstanceId, settings);
                }
            }

            return true;
        }

        private bool CopyFormInstance(Guid systemActionInstanceId, string settings)
        {
            bool start = false;

            dynamic oSettings = JsonConvert.DeserializeObject<ExpandoObject>(settings, new ExpandoObjectConverter());

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_CopyFormInstance", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@KeySource", SqlDbType.VarChar, 50) { Value = oSettings.KeySource });
                    cmd.Parameters.Add(new SqlParameter("@KeyDestiny", SqlDbType.VarChar, 50) { Value = oSettings.KeyDestiny });

                    cmd.ExecuteNonQuery();
                    start = true;
                }
            }

            return start;
        }
    }
}
