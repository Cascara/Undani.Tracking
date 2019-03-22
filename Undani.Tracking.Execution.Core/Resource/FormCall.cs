﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Text;
using Undani.Tracking.Execution.Core.Infra;

namespace Undani.Tracking.Execution.Core.Resource
{
    internal class FormCall : Call
    {
        public FormCall(IConfiguration configuration) : base(configuration) { }

        public Guid GetInstance(_ActivityInstance _activityInstance, string token)
        {
            string url = "";

            if (_activityInstance.FormVersion > 0)
                url += "CreateInstance?environmentId=" + _activityInstance.EnvironmentId.ToString() + "&formId=" + _activityInstance.FormId.Value.ToString() + "&version=" + _activityInstance.FormVersion.ToString();
            else
            {
                if (_activityInstance.FormId != Guid.Empty)
                    url += "InheritInstance?formId=" + _activityInstance.FormId.Value.ToString() + "&parentInstanceId=" + _activityInstance.FormParentInstanceId.Value.ToString();
                else
                    url += "CloneInstance?instanceId=" + _activityInstance.FormParentInstanceId.Value.ToString();

                if (_activityInstance.FormReadOnly)
                    url += "&readOnly=true";
                else if (url != string.Empty)
                    url += "&readOnly=false";
            }

            url = Configuration["ApiForm"] + "/Execution/" + url;

            HttpResponseMessage response = null;
            Guid formInstanceId;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", token);
                StringContent content = new StringContent("", Encoding.UTF8, "text/plain");

                response = client.PostAsync(url, content).Result;

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception("There are not enough parameters to obtain the instance of a form");

                formInstanceId = Guid.Parse(response.Content.ReadAsStringAsync().Result.Replace("\"", ""));
            }

            if (formInstanceId == Guid.Empty)
                throw new Exception("Empty form instance");

            return formInstanceId;

        }

        public void SetReadOnly(Guid formInstanceId, string token)
        {
            string url = Configuration["ApiForm"] + "/Execution/UpdateReadOnly?instaceId=" + formInstanceId.ToString();

            HttpResponseMessage response = null;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", token);
                StringContent content = new StringContent("", Encoding.UTF8, "text/plain");

                response = client.PostAsync(url, content).Result;

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception("There are not enough parameters to obtain the instance of a form");

                string result = response.Content.ReadAsStringAsync().Result;
            }
        }

    }
}
