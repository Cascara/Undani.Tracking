using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution
{
    public class ActivityInstanceSummary
    {
        public Guid RefId { get; set; }
        public string ActivityName { get; set; }
        public string UserName { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
    }
}
