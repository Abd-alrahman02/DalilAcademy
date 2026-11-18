using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcademy.Core.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course? Course { get; set; }
        public int StudentId { get; set; }
        public Student? Student { get; set; }
        public DateTime EnrolledAt { get; set; }
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;
        public List<Payment> Payments { get; set; } = [];

    }
}
