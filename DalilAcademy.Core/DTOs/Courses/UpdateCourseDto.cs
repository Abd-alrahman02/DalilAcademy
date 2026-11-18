namespace DalilAcademy.Core.DTOs.Courses
{
    public record UpdateCourseDto(
        string Title,
        string Level,
        decimal Price,
        string? ImageUrl,
        int InstructorId,
        int? MaxStudents = null
    );
}
