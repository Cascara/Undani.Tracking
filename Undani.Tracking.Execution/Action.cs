using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution
{
    public class ActionButton
    {
        public Guid RefId { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SignatureType { get; set; }
        public string Color { get; set; }
        public string Icon { get; set; }
        public int Order { get; set; }
    }
}
