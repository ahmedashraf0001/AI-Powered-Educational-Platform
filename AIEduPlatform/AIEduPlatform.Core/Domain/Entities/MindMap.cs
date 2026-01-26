using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class MindMap : BaseEntity
    {
        public Guid SessionId { get; set; }
        public string Topic { get; set; }
        public string Nodes { get; set; } // JSON: Array of mind map nodes
        public string Connections { get; set; } // JSON: Array of connections between nodes

        public StudySession Session { get; set; }
    }
}
