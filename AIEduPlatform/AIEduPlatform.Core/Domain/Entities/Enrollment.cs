using AIEduPlatform.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class Enrollment : BaseEntity
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public DateTime EnrolledAt { get; set; }
        public EnrollmentStatus Status { get; set; }

        // Payment/Order tracking
        public Guid? OrderId { get; set; }
        public decimal AmountPaid { get; set; }

        // Refund tracking
        public DateTime? RefundedAt { get; set; }
        public decimal? RefundAmount { get; set; }
        public string? StripeRefundId { get; set; }
        public DateTime? UnenrolledAt { get; set; }

        public User Student { get; set; }
        public Course Course { get; set; }
        public Order? Order { get; set; }
    }
}
