using DalilAcademy.Core.Models;

namespace DalilAcademy.Core.DTOs.Enrollments
{
    public record EnrollmentResponseDto(
        int Id,
        int StudentId,
        string StudentName,
        int CourseId,
        string CourseTitle,
        decimal CoursePrice,
        DateTime EnrolledAt,
        EnrollmentStatus Status
    );
}
