using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Undani.Tracking.Execution.Core.Resource
{
    internal class BoxCall : Call
    {
        public BoxCall(IConfiguration configuration) : base(configuration) { }

        public bool DocxToPDF(Guid systemActionInstanceId, Guid ownerId, List<string> documentsToConvert)
        {
            var content = new
            {
                SystemActionId = systemActionInstanceId,
                OwnerId = ownerId,
                DocumentsToConvert = documentsToConvert
            };

            HttpResponseMessage response;
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                var client = new HttpClient(httpClientHandler);

                string url = "https://apikarakboxconverter.azurewebsites.net/Execution/MultiBox/DocxToPDF";
                StringContent contentJson = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
                response = client.PostAsync(url, contentJson).Result;

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception("There was an error when trying to contact box converter");
            }

            string json = response.Content.ReadAsStringAsync().Result;

            return true;
        }
    }
}
