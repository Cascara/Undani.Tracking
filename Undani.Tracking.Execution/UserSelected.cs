using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution
{
    public class UserSelected
    {
        public Guid ProcedureInstanceRefId { get; set; }
        public string Key { get; set; }
        public Guid UserId { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string EMail { get; set; }
    }
}
