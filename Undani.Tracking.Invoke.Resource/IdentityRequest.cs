﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Undani.Tracking.Invoke.Resource.Infra;

namespace Undani.Tracking.Invoke.Resource
{
    public static class IdentityRequest
    {
        public static _User CreateUser(string configuration, dynamic user)
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

                string url = Configuration.GetValue("Url:WebIdentity") + "/api/AccountService/registry";
                StringContent contentJson = new StringContent(configuration, Encoding.UTF8, "application/json");
                response = client.PostAsync(url, contentJson).Result;

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception("It was not possible to add the traceability page in box");

                return Newtonsoft.Json.JsonConvert.DeserializeObject<_User>(response.Content.ReadAsStringAsync().Result);

            }
        }
    }
}
