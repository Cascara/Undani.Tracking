﻿using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Text;
using Undani.Tracking.Execution.Core.Invoke.Infra;

namespace Undani.Tracking.Execution.Core.Invoke.Resource
{
    internal class BusCall : Call
    {
        public BusCall(IConfiguration configuration) : base(configuration) { }

        //public void SendMessage(string queueName, string content)
        //{

        //    var bus = ClientBus.Bus.Connect(Configuration["CnSrvBus"], queueName).se;

        //    bus.Send(JObject.Parse(content));
        //}

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
