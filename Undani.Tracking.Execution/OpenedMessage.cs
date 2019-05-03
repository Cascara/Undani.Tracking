using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution
{
    public class OpenedMessage
    {
        public Guid ElementInstanceRefId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
    }
}
