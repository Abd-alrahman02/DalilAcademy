using DalilAcademy.Core.DTOs.Enrollments;
using DalilAcademy.Core.DTOs.Students;

namespace DalilAcademy.Core.Service
{
    public interface IEnrollmentService
    {
        Task<List<EnrollmentResponseDto>> GetAllAsync();
        Task<EnrollmentResponseDto> GetByIdAsync(int id);
        Task<EnrollmentResponseDto> CreateAsync(CreateEnrollmentDto dto);
        Task<EnrollmentResponseDto> UpdateCourseAsync(int id, UpdateEnrollmentDto dto);
        Task DeleteAsync(int id);
        Task<List<StudentResponseDto>> GetPaidStudentsForCourseAsync(int courseId);
        Task<List<StudentCourseDto>> GetCoursesForUserAsync(int userId);
    }
}
