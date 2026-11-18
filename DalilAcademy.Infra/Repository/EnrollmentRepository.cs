using DalilAcademy.Core.Models;
using DalilAcademy.Core.Repository;
using DalilAcademy.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace DalilAcademy.Infra.Repository
{
    public class EnrollmentRepository(AppDbContext context)
        : IEnrollmentRepository
    {
        public Task<Student?> GetStudentWithCoursesByUserIdAsync(
            int userId)
        {
            return context.Students
                .AsNoTracking()
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }
    }
}