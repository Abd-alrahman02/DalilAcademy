using DalilAcademy.Core.DTOs.Students;
using DalilAcademy.Core.Exceptions;
using DalilAcademy.Core.Repository;
using DalilAcademy.Core.Service;

namespace DalilAcademy.Infra.Service
{
    public class EnrollmentService(
        IEnrollmentRepository enrollmentRepository)
        : IEnrollmentService
    {
        public async Task<List<StudentCourseDto>>
            GetCoursesForUserAsync(int userId)
        {
            var student =
                await enrollmentRepository
                    .GetStudentWithCoursesByUserIdAsync(userId);

            if (student is null)
            {
                throw new NotFoundException(
                    "Student profile was not found for the current user.");
            }

            return student.Enrollments
                .Select(e => new StudentCourseDto(
                    e.CourseId,
                    e.Course!.Title,
                    e.Course.Price,
                    e.EnrolledAt
                ))
                .ToList();
        }
    }
}