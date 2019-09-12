using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Undani.Tracking.Execution.Core.Invoke.Infra;

namespace Undani.Tracking.Execution.Core.Invoke.Resource
{
    internal class IdentityCall : Call
    {
        public IdentityCall(IConfiguration configuration) : base(configuration) { }

        public _User CreateUser(string content)
        {           

            using (var client = new HttpClient())
            {
                HttpResponseMessage response;

                string url = Configuration["WebIdentity"] + "/api/AccountService/registry";
                StringContent contentJson = new StringContent(content, Encoding.UTF8, "application/json");
                response = client.PostAsync(url, contentJson).Result;

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception("It was not possible to add the traceability page in box");

                _User _user = JsonConvert.DeserializeObject<_User>(response.Content.ReadAsStringAsync().Result);

                return _user;
            }
        }
    }
}
