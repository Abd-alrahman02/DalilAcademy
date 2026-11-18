namespace DalilAcademy.Core.DTOs.Courses
{
    public record CourseResponseDto(
        int Id,
        string Title,
        string Level,
        decimal Price,
        string? ImageUrl,
        int InstructorId,
        string? InstructorName,
        int? MaxStudents
    );
}
