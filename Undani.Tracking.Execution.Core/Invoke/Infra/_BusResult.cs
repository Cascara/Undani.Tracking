using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution.Core.Invoke.Infra
{
    internal class _BusResult
    {
        public string TargetQueue { get; set; }
        public bool IsSuccess { get; set; }
        public List<string> Errors { get; set; }
    }
}
