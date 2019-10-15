using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution
{
    public class Message
    {
        public Guid Id { get; set; }
        public string ActivityName { get; set; }
        public string FlowName { get; set; }
        public string FlowInstanceKey { get; set; }
        public dynamic FlowInstanceContent { get; set; }
        public string ProcedureInstanceKey { get; set; }
        public dynamic ProcedureInstanceContent { get; set; }
        public dynamic StatesFlowInstance { get; set; }
        public dynamic StatesProcedureInstance { get; set; }
        public DateTime Start { get; set; }
        public bool Viewed { get; set; }
        public int ActivityUserGroupTypeId { get; set; }
        public int Status { get; set; }
    }
}
