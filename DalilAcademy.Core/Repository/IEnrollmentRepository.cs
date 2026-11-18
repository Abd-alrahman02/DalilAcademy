using DalilAcademy.Core.Models;

namespace DalilAcademy.Core.Repository
{
    public interface IEnrollmentRepository
    {
        Task<Student?> GetStudentWithCoursesByUserIdAsync(
            int userId);
    }
}