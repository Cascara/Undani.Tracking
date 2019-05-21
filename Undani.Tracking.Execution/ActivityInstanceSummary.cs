using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution
{
    public class ActivityInstanceSummary
    {
        public Guid RefId { get; set; }
        public string Name { get; set; }
        public string CoustomViewer { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public string Days { get; set; }
        public string Hours { get; set; }
        public string Reference { get; set; }
    }
}
