using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution
{
    public class User
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public string Reference { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string EMail { get; set; }
        public dynamic Content { get; set; }
    }
}
