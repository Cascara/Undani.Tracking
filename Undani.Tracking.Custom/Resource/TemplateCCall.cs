﻿using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Undani.Tracking.Custom.Resource
{
    public class TemplateCall : Call
    {
        public TemplateCall(IConfiguration configuration) : base(configuration) { }

        public void Notification(string content, string token)
        {
            HttpResponseMessage response;
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                var client = new HttpClient(httpClientHandler);

                client.DefaultRequestHeaders.Add("Authorization", token);

                string url = Configuration["ApiTemplate"] + "/Execution/Template/Notification";
                StringContent contentJson = new StringContent(content, Encoding.UTF8, "application/json");
                response = client.PostAsync(url, contentJson).Result;

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception("There was an error when trying to contact template");
            }

            string json = response.Content.ReadAsStringAsync().Result;

            dynamic responseResult = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());

            if (responseResult.IsError)
                throw new Exception("There was an error when execute the template");
        }
    }
}