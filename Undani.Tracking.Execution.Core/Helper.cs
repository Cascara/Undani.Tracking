using Microsoft.Extensions.Configuration;
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
        private IConfiguration configuration;

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
    }
}
