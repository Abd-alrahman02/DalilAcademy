namespace DalilAcademy.Core.DTOs.Students
{
    public record StudentCourseDto(
        int CourseId,
        string Title,
        decimal Price,
        DateTime EnrolledAt
    );
}