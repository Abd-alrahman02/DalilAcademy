namespace DalilAcademy.Core.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int? MaxStudents { get; set; }
        public int InstructorId { get; set; }
        public string? ImageUrl { get; set; }
        public Instructor? Instructor { get; set; }
        public List<Enrollment> Enrollments { get; set; } = [];
    }
}
