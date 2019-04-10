using Microsoft.Extensions.Configuration;
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

        public _User CreateUser(string configuration, dynamic user)
        {
            configuration = configuration.Replace("[Email]", user.Integration.Carta.Usuario);
            configuration = configuration.Replace("[Password]", user.Integration.Carta.Confirmar);
            configuration = configuration.Replace("[GivenName]", user.Integration.Datos.Nombre);
            configuration = configuration.Replace("[FamilyName]", user.Integration.Datos.PrimerApellido + ' ' + user.Integration.Datos.SegundoApellido);

            Guid ownerId = user.OwnerId;
            configuration = configuration.Replace("[OwnerId]", ownerId.ToString());

            using (var client = new HttpClient())
            {
                HttpResponseMessage response;

                string url = Configuration["WebIdentity"] + "/api/AccountService/registry";
                StringContent contentJson = new StringContent(configuration, Encoding.UTF8, "application/json");
                response = client.PostAsync(url, contentJson).Result;

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception("It was not possible to add the traceability page in box");

                _User _user = Newtonsoft.Json.JsonConvert.DeserializeObject<_User>(response.Content.ReadAsStringAsync().Result);
                _user.OwnerId = ownerId;
                _user.RFC = user.Integration.Datos.RFC;

                return _user;
            }
        }
    }
}
