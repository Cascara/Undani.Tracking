using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution
{
    public class SystemActionInstanceResponse
    {
        public Guid SystemActionInstanceId { get; set; }
        public string ProcedureInstanceContent { get; set; }
        public string FlowInstanceContent { get; set; }

    }
}
