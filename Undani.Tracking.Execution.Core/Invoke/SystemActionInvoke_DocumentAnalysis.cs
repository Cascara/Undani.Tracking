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
        public bool DocumentAnalysis(Guid systemActionInstanceId, string alias, string settings, bool isStrict)
        {
            bool start = false;
            switch (alias)
            {
                
                case "ProcedureInstanceContent":
                    start = DAOnProcedureInstanceContent(systemActionInstanceId, settings);
                    break;

                default:
                    throw new NotImplementedException();
            }

            return start;
        }
               
        private bool DAOnProcedureInstanceContent(Guid systemActionInstanceId, string settings)
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
    }
}
