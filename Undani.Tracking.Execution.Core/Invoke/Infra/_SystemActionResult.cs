using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution.Core.Invoke.Infra
{
    public class _SystemActionResult
    {
        public _SystemActionResult()
        {
            Success = false;
        }
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}
