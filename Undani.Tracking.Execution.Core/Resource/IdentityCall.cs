using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Dynamic;
using System.Security.Claims;
using Undani.JWT;

namespace Undani.Tracking.Execution.Core.Resource
{
    internal class IdentityCall : Call
    {
        public IdentityCall(IConfiguration configuration) : base(configuration) { }

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
