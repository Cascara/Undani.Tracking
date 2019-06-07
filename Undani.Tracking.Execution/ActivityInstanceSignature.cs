using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution
{
    public class ActivityInstanceSignature
    {
        public ActivityInstanceSignature()
        {
            ElementsSignatures = new List<ElementSignature>();
        }

        public Guid RefId { get; set; }
        public Guid FormInstanceId { get; set; }
        public string ElementId { get; set; }
        public Guid EnvironmentId { get; set; }
        public Guid ProcedureInstanceRefId { get; set; }
        public List<ElementSignature> ElementsSignatures { get; set; }
    }
}
