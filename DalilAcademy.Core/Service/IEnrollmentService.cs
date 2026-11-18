using DalilAcademy.Core.DTOs.Students;

namespace DalilAcademy.Core.Service
{
    public interface IEnrollmentService
    {
        Task<List<StudentCourseDto>> GetCoursesForUserAsync(
            int userId);
    }
}