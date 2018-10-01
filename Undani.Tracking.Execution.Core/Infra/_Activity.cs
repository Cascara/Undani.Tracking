using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution.Core.Infra
{
    class _ActivityInstance
    {

        public string Id { get; set; }
        public int ActivityInstanceId { get; set; }
        public int FlowInstanceId { get; set; }
        public Guid UserId { get; set; }
        public Guid? ActionInstanceId { get; set; }
        public Guid? ActionRefId { get; set; }
        public Guid EnvironmentId { get; set; }
        public Guid? FormId { get; set; }
        public int FormVersionId { get; set; }
        public Guid? FormParentInstanceId { get; set; }
        public Guid? FormInstanceId { get; set; }
    }
}
