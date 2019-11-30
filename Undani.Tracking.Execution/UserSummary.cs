using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution
{
    public class UserSummary
    {
        public Guid Id { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string EMail { get; set; }
    }
}
