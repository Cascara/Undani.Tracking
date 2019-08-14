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
using Newtonsoft.Json;
using System.Dynamic;
using Newtonsoft.Json.Converters;
using Undani.Tracking.Execution.Core;

namespace Undani.Tracking.Core.Invoke
{
    public partial class SystemActionInvoke
    {
        public bool Integration(Guid systemActionInstanceId, string alias, string configuration)
        {
            bool start = false;
            switch (alias)
            {
                case "FormInstanceIntegration":
                    start = FormInstanceIntegration(systemActionInstanceId, configuration);
                    break;

                case "Custom_Credere":
                    start = Custom_Credere(systemActionInstanceId, configuration);
                    break;

                case "Custom_AssignEvaluator":
                    start = Custom_AssignEvaluator(systemActionInstanceId, configuration);
                    break;

                case "Custom_AssignAuthorizator":
                    start = Custom_AssignAuthorizator(systemActionInstanceId, configuration);
                    break;

                case "Custom_CredereState":
                    start = Custom_CredereState(systemActionInstanceId, configuration);
                    break;

                case "Custom_MailEvaluator":
                    start = Custom_MailEvaluator(systemActionInstanceId, configuration);
                    break;

                default:
                    throw new Exception("The method is not implemented");
            }

            return start;
        }

        private bool FormInstanceIntegration(Guid systemActionInstanceId, string configuration)
        {
            BusCall busCall = new BusCall(Configuration);

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("EXECUTION.usp_Set_SAI_FormInstanceIntegration", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@FormInstanceId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    configuration = configuration.Replace("[FormInstanceId]", cmd.Parameters["@FormInstanceId"].Value.ToString());

                    busCall.SendMessage(configuration);
                }
            }            

            return true;
        }

        private bool Custom_Credere(Guid systemActionInstanceId, string configuration)
        {
            BusCall busCall = new BusCall(Configuration);

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("CUSTOM.usp_Set_SAI_Credere", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@FormInstanceId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceStartDate", SqlDbType.DateTime) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    DateTime startDate = (DateTime)cmd.Parameters["@ProcedureInstanceStartDate"].Value;

                    configuration = configuration.Replace("[FormInstanceId]", cmd.Parameters["@FormInstanceId"].Value.ToString());
                    configuration = configuration.Replace("[SystemActionInstranceId]", systemActionInstanceId.ToString());
                    configuration = configuration.Replace("[FechaRecepcion]", startDate.ToString("dd/MM/yyyy"));
                }
            }

            busCall.SendMessage(configuration);

            return true;
        }

        private bool Custom_AssignEvaluator(Guid systemActionInstanceId, string configuration)
        {
            bool start = false;

            string json = new FormCall(Configuration).GetInstanceObject(systemActionInstanceId, Token);

            dynamic oJson = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());

            string municipalityId = oJson.Integration.CentroTrabajo.Municipio.id;

            string federalEntityId = oJson.Integration.CentroTrabajo.Entidad.id;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("CUSTOM.usp_Set_SAI_AssignEvaluator", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@MunicipalityId", SqlDbType.VarChar, 3) { Value = municipalityId });
                    cmd.Parameters.Add(new SqlParameter("@FederalEntityId", SqlDbType.VarChar, 2) { Value = federalEntityId });

                    cmd.ExecuteNonQuery();

                    start = true;
                }
            }

            return start;
        }

        private bool Custom_AssignAuthorizator(Guid systemActionInstanceId, string configuration)
        {
            bool start = false;

            string json = new FormCall(Configuration).GetInstanceObject(systemActionInstanceId, Token);

            dynamic oJson = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());

            string municipalityId = oJson.Integration.CentroTrabajo.Municipio.id;

            string federalEntityId = oJson.Integration.CentroTrabajo.Entidad.id;

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("CUSTOM.usp_Set_SAI_AssignAuthorizator", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@MunicipalityId", SqlDbType.VarChar, 3) { Value = municipalityId });
                    cmd.Parameters.Add(new SqlParameter("@FederalEntityId", SqlDbType.VarChar, 2) { Value = federalEntityId });

                    cmd.ExecuteNonQuery();

                    start = true;
                }
            }

            return start;
        }

        private bool Custom_CredereState(Guid systemActionInstanceId, string configuration)
        {
            BusCall busCall = new BusCall(Configuration);

            using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
            {
                cn.Open();

                using (SqlCommand cmd = new SqlCommand("CUSTOM.usp_Set_SAI_CredereState", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                    cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceContent", SqlDbType.VarChar, 2000) { Direction = ParameterDirection.Output });

                    cmd.ExecuteNonQuery();

                    dynamic oJson = JsonConvert.DeserializeObject<ExpandoObject>((string)cmd.Parameters["@ProcedureInstanceContent"].Value, new ExpandoObjectConverter());

                    configuration = configuration.Replace("[SystemActionInstranceId]", systemActionInstanceId.ToString());
                    configuration = configuration.Replace("[NumeroCliente]", oJson.SAIResponse.folioClienteField);
                }
            }

            busCall.SendMessage(configuration);

            return true;
        }

        private bool Custom_MailEvaluator(Guid systemActionInstanceId, string configuration)
        {
            bool start = false;

            try
            {
                using (SqlConnection cn = new SqlConnection(Configuration["CnDbTracking"]))
                {
                    cn.Open();

                    using (SqlCommand cmd = new SqlCommand("CUSTOM.usp_Set_SAI_MailEvaluator", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@SystemActionInstanceId", SqlDbType.UniqueIdentifier) { Value = systemActionInstanceId });
                        cmd.Parameters.Add(new SqlParameter("@EnvironmentId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });
                        cmd.Parameters.Add(new SqlParameter("@ProcedureInstanceKey", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });
                        cmd.Parameters.Add(new SqlParameter("@UserContent", SqlDbType.VarChar, 2000) { Direction = ParameterDirection.Output });
                        cmd.Parameters.Add(new SqlParameter("@EvaluatorMail", SqlDbType.VarChar, 256) { Direction = ParameterDirection.Output });

                        cmd.ExecuteNonQuery();

                        dynamic oJson = JsonConvert.DeserializeObject<ExpandoObject>((string)cmd.Parameters["@UserContent"].Value, new ExpandoObjectConverter());

                        configuration = configuration.Replace("[SystemActionInstanceId]", systemActionInstanceId.ToString());
                        configuration = configuration.Replace("[EnvironmentId]", cmd.Parameters["@EnvironmentId"].Value.ToString());
                        configuration = configuration.Replace("[NumeroTramite]", (string)cmd.Parameters["@ProcedureInstanceKey"].Value);
                        configuration = configuration.Replace("[NombreCentroTrabajo]", (string)oJson.nombreCTField);
                        configuration = configuration.Replace("[RegistroPatronal]", (string)oJson.registroPatronalField);
                        configuration = configuration.Replace("[CorreoDestinatarios]", (string)cmd.Parameters["@EvaluatorMail"].Value);
                    }
                }

                start = new TemplateCall(Configuration).Notification(configuration, Token);
            }
            catch (Exception)
            {

            }           

            return true;
        }
    }
}
