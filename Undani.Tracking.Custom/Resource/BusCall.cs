using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Undani.Tracking.Custom.Infra;

namespace Undani.Tracking.Custom.Resource
{
    public class BusCall : Call
    {
        public BusCall(IConfiguration configuration) : base(configuration) { }

        public void SendMessage(string content)
        {
            using (var client = new HttpClient())
            {
                HttpResponseMessage httpResponse;

                string url = Configuration["ApiBus"] + "/api/message/send";
                StringContent contentJson = new StringContent(content, Encoding.UTF8, "application/json");
                httpResponse = client.PostAsync(url, contentJson).Result;

                if (httpResponse.StatusCode != HttpStatusCode.OK)
                    throw new Exception("It was not possible to contact undani bus");

                string json = httpResponse.Content.ReadAsStringAsync().Result;

                _BusResult result = JsonConvert.DeserializeObject<_BusResult>(json);

                if (!result.IsSuccess)
                    throw new Exception("There was an error when trying to send a message");
            }

        }
    }
}
