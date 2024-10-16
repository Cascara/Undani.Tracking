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
                
                case "DAOnProcedureInstanceContent":
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

            settings = settings.Replace("{{SystemActionInstanceId}}", systemActionInstanceId.ToString());


            string items = "";

            string value = "";

            if (joFormInstance.SelectToken("$.Integration.CentroTrabajo.DocumentosCT[?(@.Tipo.id=='5')].Archivo.SystemName") != null)
            {
                if (joFormInstance.SelectToken("$.Integration.CentroTrabajo.DocumentosCT[?(@.Tipo.id=='5')].Archivo.SystemName").ToString() != "")
                {
                    value = "{\"UrlBoxFile\":\"{{ApiBox}}/Execution/Box/Download?systemName={{Identificacion}}\",\"Elements\":[{\"Entity\":\"DocumentosCT.Tipo.id:5\",\"Value\":\"{{Nombre}} {{PrimerApellido}} {{SegundoApellido}}\"}]}";

                    value = value.Replace("{{Identificacion}}", joFormInstance.SelectToken("$.Integration.CentroTrabajo.DocumentosCT[?(@.Tipo.id=='5')].Archivo.SystemName").ToString());

                    value = value.Replace("{{Nombre}}", joFormInstance.SelectToken("$.Integration.CentroTrabajo.Nombre").ToString());
                    value = value.Replace("{{PrimerApellido}}", joFormInstance.SelectToken("$.Integration.CentroTrabajo.PrimerApellido").ToString());
                    value = value.Replace("{{SegundoApellido}}", joFormInstance.SelectToken("$.Integration.CentroTrabajo.SegundoApellido").ToString());

                    items += "," + value;
                }                
            }

            if (joFormInstance.SelectToken("$.Integration.CentroTrabajo.DocumentosCT[?(@.Tipo.id=='3')].Archivo.SystemName") != null)
            {
                if (joFormInstance.SelectToken("$.Integration.CentroTrabajo.DocumentosCT[?(@.Tipo.id=='3')].Archivo.SystemName").ToString() != "")
                {
                    value = "{\"UrlBoxFile\":\"{{ApiBox}}/Execution/Box/Download?systemName={{Comprobante}}\",\"Elements\":[{\"Entity\":\"DocumentosCT.Tipo.id:3\",\"Value\":\"{{Calle}} {{NumeroExterior}} {{CodigoPostal}} {{Colonia}} {{Municipio}} {{Entidad}}\"}]}";

                    value = value.Replace("{{Comprobante}}", joFormInstance.SelectToken("$.Integration.CentroTrabajo.DocumentosCT[?(@.Tipo.id=='3')].Archivo.SystemName").ToString());

                    value = value.Replace("{{Calle}}", joFormInstance.SelectToken("$.Integration.CentroTrabajo.Calle").ToString().Replace("\"", ""));
                    value = value.Replace("{{NumeroExterior}}", joFormInstance.SelectToken("$.Integration.CentroTrabajo.NumeroExterior").ToString().Replace("\"", ""));
                    value = value.Replace("{{CodigoPostal}}", joFormInstance.SelectToken("$.Integration.CentroTrabajo.CodigoPostal").ToString().Replace("\"", ""));
                    value = value.Replace("{{Colonia}}", joFormInstance.SelectToken("$.Integration.CentroTrabajo.Colonia.text").ToString().Replace("\"", ""));
                    value = value.Replace("{{Municipio}}", joFormInstance.SelectToken("$.Integration.CentroTrabajo.Municipio.text").ToString());
                    value = value.Replace("{{Entidad}", joFormInstance.SelectToken("$.Integration.CentroTrabajo.Entidad.text").ToString());

                    items += "," + value;
                }                
            }

            if (joFormInstance.SelectToken("$.Integration.Representante.DocumentosRL[?(@.Tipo.id=='5')].Archivo.SystemName") != null)
            {
                if (joFormInstance.SelectToken("$.Integration.Representante.DocumentosRL[?(@.Tipo.id=='5')].Archivo.SystemName").ToString() != "")
                {
                    value = "{\"UrlBoxFile\":\"{{ApiBox}}/Execution/Box/Download?systemName={{Identificacion}}\",\"Elements\":[{\"Entity\":\"DocumentosRL.Tipo.id:5\",\"Value\":\"{{Nombre}} {{PrimerApellido}} {{SegundoApellido}}\"}]}";

                    value = value.Replace("{{Identificacion}}", joFormInstance.SelectToken("$.Integration.Representante.DocumentosRL[?(@.Tipo.id=='5')].Archivo.SystemName").ToString());

                    value = value.Replace("{{Nombre}}", joFormInstance.SelectToken("$.Integration.Representante.RLNombre").ToString());
                    value = value.Replace("{{PrimerApellido}}", joFormInstance.SelectToken("$.Integration.Representante.RLPrimerApellido").ToString());
                    value = value.Replace("{{SegundoApellido}}", joFormInstance.SelectToken("$.Integration.Representante.RLSegundoApellido").ToString());

                    items += "," + value;
                }
            }

            settings = settings.Replace("{{Items}}", items.Substring(1));
            settings = settings.Replace("{{ApiBox}}", Configuration["ApiBox"]);

            BusCall busCall = new BusCall(Configuration);

            busCall.SendMessage("adi", settings);

            SetConfiguration(systemActionInstanceId, settings);

            return true;
        }
    }
}
