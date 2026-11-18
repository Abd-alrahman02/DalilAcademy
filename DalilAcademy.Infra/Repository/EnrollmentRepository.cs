using DalilAcademy.Core.Exceptions;
using DalilAcademy.Core.Models;
using DalilAcademy.Core.Repository;
using DalilAcademy.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DalilAcademy.Infra.Repository
{
    public class EnrollmentRepository(AppDbContext context)
        : IEnrollmentRepository
    {
        public Task<List<Enrollment>> GetAllAsync()
        {
            return context.Enrollments
                .AsNoTracking()
                .Include(e => e.Student)
                .Include(e => e.Course)
                .OrderBy(e => e.Id)
                .ToListAsync();
        }

        public Task<Enrollment?> GetByIdAsync(int id)
        {
            return context.Enrollments
                .AsNoTracking()
                .Include(e => e.Student)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Enrollment> CreateAsync(Enrollment enrollment)
        {
            context.Enrollments.Add(enrollment);
            await SaveAsync();

            return await GetByIdAsync(enrollment.Id)
                ?? throw new InvalidOperationException(
                    "The enrollment could not be reloaded after creation.");
        }

        public async Task<Enrollment?> UpdateCourseAsync(
            int id,
            int courseId,
            EnrollmentStatus status)
        {
            var enrollment = await context.Enrollments
                .Include(e => e.Payments)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enrollment is null) return null;

            if (enrollment.CourseId != courseId)
            {
                foreach (var payment in enrollment.Payments.Where(
                    payment => payment.Status == PaymentStatus.Created))
                {
                    payment.Status = PaymentStatus.Failed;
                }
            }

            enrollment.CourseId = courseId;
            enrollment.Status = status;
            await SaveAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var enrollment = await context.Enrollments
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enrollment is null) return false;

            context.Enrollments.Remove(enrollment);
            await SaveAsync();
            return true;
        }

        public Task<List<Student>> GetPaidStudentsForCourseAsync(int courseId)
        {
            return context.Enrollments
                .AsNoTracking()
                .Where(e => e.CourseId == courseId &&
                            e.Status == EnrollmentStatus.Paid)
                .Select(e => e.Student!)
                .OrderBy(s => s.Id)
                .ToListAsync();
        }

        public Task<int> CountPaidEnrollmentsForCourseAsync(int courseId)
        {
            return context.Enrollments.CountAsync(e =>
                e.CourseId == courseId &&
                e.Status == EnrollmentStatus.Paid);
        }

        public Task<Student?> GetStudentWithCoursesByUserIdAsync(int userId)
        {
            return context.Students
                .AsNoTracking()
                .Include(s => s.Enrollments
                    .Where(e => e.Status == EnrollmentStatus.Paid))
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        private async Task SaveAsync()
        {
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation
                })
            {
                throw new ConflictException(
                    "The student is already enrolled in this course.");
            }
        }
    }
}
