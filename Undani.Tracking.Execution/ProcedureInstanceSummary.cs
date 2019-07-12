using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution
{
    public class ProcedureInstanceSummary
    {
        public Guid RefId { get; set; }
        public Guid ProcedureRefId { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public dynamic Content { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public Guid EnvironmentId { get; set; }
        public dynamic States { get; set; }
        public dynamic FormInstances { get; set; }
        public string DocumentsSignedZiped { get; set; }
        public dynamic DocumentsSigned { get; set; }
    }
}
