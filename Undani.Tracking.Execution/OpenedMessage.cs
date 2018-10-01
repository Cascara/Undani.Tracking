using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution
{
    public class OpenedMessage
    {
        public Guid ActivityIntanceRefId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
    }
}
