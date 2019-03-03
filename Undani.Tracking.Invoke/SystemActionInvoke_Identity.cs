using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Undani.Tracking.Invoke.Resource;
using Undani.Tracking.Invoke.Resource.Infra;

namespace Undani.Tracking.Invoke
{
    public static partial class SystemActionInvoke
    {
        public static bool Identity(Guid systemActionInstanceId, string alias, string configuration)
        {
            bool start = false;
            switch (alias)
            {
                case "CreateUser":
                    start = CreateUser(systemActionInstanceId, alias, configuration);
                    break;

                default:
                    throw new Exception("The method is not implemented");
            }

            return start;
        }

        private static bool CreateUser(Guid systemActionInstanceId, string alias, string configuration)
        {
            bool start = false;

            dynamic obj = FormRequest.GetInstanceObject(systemActionInstanceId);

            _User _user = IdentityRequest.CreateUser(configuration, obj);

            if (_user.SubjectId != Guid.Empty)
                start = true;

            return start;
        }
    }
}
