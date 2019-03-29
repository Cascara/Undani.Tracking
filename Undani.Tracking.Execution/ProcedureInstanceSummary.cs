using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution
{
    public class ProcedureInstanceSummary
    {
        public Guid RefId { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public dynamic Content { get; set; }
        public DateTime Start { get; set; }
        public Guid EnvironmentId { get; set; }
        public string PrincipalState { get; set; }
    }
}
