﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution
{
    public class ProcedureInstanceSummary
    {
        public Guid RefId { get; set; }
        public string ProcedureName { get; set; }
        public string Key { get; set; }
        public dynamic Content { get; set; }
        public DateTime Created { get; set; }
    }
}
