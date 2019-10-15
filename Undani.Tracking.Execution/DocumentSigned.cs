using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution
{
    public class DocumentSigned
    {
        public string SystemName { get; set; }
        public string OriginalName { get; set; }
        public string HashCode { get; set; }
        public bool Creted { get; set; }
    }
}
