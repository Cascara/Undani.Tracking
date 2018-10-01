using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution
{
    public class FlowInstance
    {
        public Guid RefId { get; set; }
        public string FlowName { get; set; }
        public string Key { get; set; }
        public dynamic Content { get; set; }
        public Guid EnvironmentId { get; set; }
        public DateTime Created { get; set; }
        public dynamic States { get; set; }
        public List<ActivityInstanceSummary> ActivityInstances { get; set; }
    }
}
