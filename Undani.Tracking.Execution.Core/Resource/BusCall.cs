using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using Undani.Tracking.Execution.Core.Invoke.Infra;

namespace Undani.Tracking.Execution.Core.Resource
{
    internal class BusCall : Call
    {
        public BusCall(IConfiguration configuration) : base(configuration) { }

        public void SendMessage(string queueName, string message)
        {
            var queueClient = ClientBus.Bus.Connect(Configuration["CnSrvBus"], queueName);

            queueClient.Send(JObject.Parse(message));
        }
    }
}
