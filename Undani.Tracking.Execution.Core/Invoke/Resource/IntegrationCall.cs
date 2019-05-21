using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Undani.Tracking.Execution.Core.Invoke.Resource
{
    public class IntegrationCall : Call
    {
        public IntegrationCall(IConfiguration configuration) : base(configuration) { }

        public bool ExecuteFormInstanceIntegration(string content)
        {
            HttpResponseMessage response;
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                var client = new HttpClient(httpClientHandler);

                string url = Configuration["ApiBus"] + "/api/message/send";
                StringContent contentJson = new StringContent(content, Encoding.UTF8, "application/json");
                response = client.PostAsync(url, contentJson).Result;

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception("There was an error when trying to contact the service bus");
            }

            string json = response.Content.ReadAsStringAsync().Result;

            dynamic responseResult = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());

            return responseResult.IsSuccess;
        }
    }
}
