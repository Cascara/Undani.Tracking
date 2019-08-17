using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Undani.Tracking.Custom.Resource
{
    public static class TemplateCall
    {
        public static void Notification(string apiTemplate, string content, string token)
        {
            HttpResponseMessage response;
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                var client = new HttpClient(httpClientHandler);

                client.DefaultRequestHeaders.Add("Authorization", token);

                string url = apiTemplate + "/Execution/Template/Notification";
                StringContent contentJson = new StringContent(content, Encoding.UTF8, "application/json");
                response = client.PostAsync(url, contentJson).Result;

                //if (response.StatusCode != HttpStatusCode.OK)
                //    throw new Exception("There was an error when trying to contact template");
            }

            string result = response.Content.ReadAsStringAsync().Result;
        }
    }
}
