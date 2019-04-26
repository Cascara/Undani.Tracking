﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution
{
    public class ProcedureInstanceCreated
    {
        public Guid ProcedureInstanceRefId { get; set; }
        public Guid ActivityInstanceRefId { get; set; }
        public int ProcedureBehaviorTypeId { get; set; }
    }
}
