using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution
{
    public class ProcedureInstance
    {
        public Guid RefId { get; set; }
        public string ProcedureName { get; set; }
        public string ProcedureKey { get; set; }
        public string Key { get; set; }
        public dynamic Content { get; set; }
        public dynamic States { get; set; }
        public List<ActivityInstanceSummary> ActivityInstances { get; set; }
        public DateTime Created { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid EnvironmentId { get; set; }
        public dynamic DocumentsSigned { get; set; }
    }
}
