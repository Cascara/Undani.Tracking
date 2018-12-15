using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution
{
    public class FlowInstanceSummary
    {
        public FlowInstanceSummary()
        {
            this.ProcedureInstance = new ProcedureInstance();
        }
        public Guid RefId { get; set; }
        public string Name { get; set; } 
        public string Key { get; set; }
        public dynamic Content { get; set; }
        public DateTime Created { get; set; }
        public ProcedureInstance ProcedureInstance { get; set; }
    }
}
