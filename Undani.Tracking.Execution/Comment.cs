using System;
using System.Collections.Generic;
using System.Text;

namespace Undani.Tracking.Execution
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Text { get; set; }
        public DateTime Created { get; set; }
        public bool IsMe { get; set; }
    }
}
