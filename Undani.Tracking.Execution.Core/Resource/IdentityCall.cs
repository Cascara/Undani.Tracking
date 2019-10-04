using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using Undani.JWT;
using Undani.Tracking.Execution.Core.Invoke.Infra;

namespace Undani.Tracking.Execution.Core.Resource
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

        public string GetAnonymousToken()
        {
            dynamic userAnonymous = JsonConvert.DeserializeObject<ExpandoObject>(Configuration["DataAnonymous"], new ExpandoObjectConverter());

            var claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.Name, userAnonymous.Name));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userAnonymous.NameIdentifier));
            claims.Add(new Claim(ClaimTypes.Email, userAnonymous.Email));
            claims.Add(new Claim(ClaimTypes.GroupSid, userAnonymous.Email));

            var _Identity = new ClaimsIdentity(claims, "Basic");

            return JWToken.Token(_Identity);
        }
    }
}
