using DalilAcademy.Core.Models;

namespace DalilAcademy.Core.Repository
{
    public interface IEnrollmentRepository
    {
        Task<List<Enrollment>> GetAllAsync();
        Task<Enrollment?> GetByIdAsync(int id);
        Task<Enrollment> CreateAsync(Enrollment enrollment);
        Task<Enrollment?> UpdateCourseAsync(int id, int courseId);
        Task<bool> DeleteAsync(int id);
        Task<List<Student>> GetPaidStudentsForCourseAsync(int courseId);
        Task<int> CountPaidEnrollmentsForCourseAsync(int courseId);
        Task<Student?> GetStudentWithCoursesByUserIdAsync(int userId);
    }
}
