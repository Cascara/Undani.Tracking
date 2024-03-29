﻿using System;
using System.Collections.Generic;

namespace Undani.Tracking.Execution
{
    public class ActivityInstance
    {
        public ActivityInstance()
        {
            ActionButtons = new List<ActionButton>();
        }
        public Guid RefId { get; set; }
        public string Name { get; set; }
        public string CatalogId { get; set; }
        public string CoustomViewer { get; set; }
        public bool ActionButtonsDisabled { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public Guid FormInstanceId { get; set; }
        public bool FormReadOnly { get; set; }
        public List<ActionButton> ActionButtons { get; set; }
        public FlowInstanceSummary FlowInstanceSummary { get; set; }
        public string BeforeSignScript { get; set; }
        public dynamic DocumentsSigned { get; set; }
    }
}
