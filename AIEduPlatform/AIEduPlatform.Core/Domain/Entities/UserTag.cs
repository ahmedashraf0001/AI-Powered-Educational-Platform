using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class UserTag
    {
        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid TagId { get; set; }
        public Tag Tag { get; set; }

        public double Weight { get; set; } // interest strength (0 → 1)
        public DateTime LastUpdated { get; set; }
        public TagSource Source { get; set; } // Manual | Derived
    }
    public enum TagSource
    {
        Manual,
        Derived
    }
}
