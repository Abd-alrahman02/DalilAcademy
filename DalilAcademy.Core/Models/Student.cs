using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcademy.Core.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;    
        public string Email { get; set; } = string.Empty;
        public List<Enrollment> Enrollments { get; set; } = [];
        public int? UserId { get; set; }
        public User? User { get; set; }

    }
}
