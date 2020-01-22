using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Undani.Tracking.Execution.API.Model
{
    public class SystemActionInstanceResponse
    {
        public Guid SystemActionInstanceId { get; set; }
        public string ProcedureInstanceContent { get; set; }
        public string FlowInstanceContent { get; set; }
    }
}
