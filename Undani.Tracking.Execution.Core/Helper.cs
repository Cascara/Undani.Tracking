using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Undani.Tracking.Execution.Core.Resource;

namespace Undani.Tracking.Execution.Core
{
    public abstract class Helper
    {
        private IConfiguration _configuration;
        private Guid _userId;
        private string _token;

        public Helper(IConfiguration configuration, Guid userId, string token = "")
        {
            _configuration = configuration;
            _userId = userId;
            _token = token;
        }

        public IConfiguration Configuration
        {
            get { return _configuration; }
        }

        public Guid UserId
        {
            get { return _userId; }
        }

        public string Token
        {
            get
            {
                if (_userId == Guid.Empty)
                {
                    string token = new IdentityCall(_configuration).GetAnonymousToken();
                    _token = "Bearer " + token;
                }
                return _token;
            }
        }

        internal string GetDocumentsSignedZiped(string documentsSigned)
        {
            JObject jObject = JObject.Parse(documentsSigned);

            JEnumerable<JToken> jTokens = jObject.Children();

            JArray jArray;
            string documents = "";
            foreach (JToken jToken in jTokens)
            {
                jArray = (JArray)jObject[jToken.Path];
                for (int i = 0; i < jArray.Count; i++)
                {
                    documents += "," + jArray[i]["SystemName"];
                }
            }

            return documents != "" ? documents.Substring(1) : "";
        }
    }
}
