namespace DalilAcademy.Core.DTOs.Courses
{
    public record CreateCourseDto(
        string Title,
        string Level,
        decimal Price,
        string? ImageUrl,
        int InstructorId,
        int? MaxStudents = null
    );
}
