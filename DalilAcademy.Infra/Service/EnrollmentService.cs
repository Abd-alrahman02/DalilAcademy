using DalilAcademy.Core.DTOs.Enrollments;
using DalilAcademy.Core.DTOs.Students;
using DalilAcademy.Core.Exceptions;
using DalilAcademy.Core.Mappings;
using DalilAcademy.Core.Models;
using DalilAcademy.Core.Repository;
using DalilAcademy.Core.Service;

namespace DalilAcademy.Infra.Service
{
    public class EnrollmentService(
        IEnrollmentRepository enrollmentRepository,
        IStudentRepository studentRepository,
        ICourseRepository courseRepository) : IEnrollmentService
    {
        public async Task<List<EnrollmentResponseDto>> GetAllAsync()
        {
            var enrollments = await enrollmentRepository.GetAllAsync();
            return enrollments.Select(ToDto).ToList();
        }

        public async Task<EnrollmentResponseDto> GetByIdAsync(int id)
        {
            return ToDto(await GetEnrollmentOrThrowAsync(id));
        }

        public async Task<EnrollmentResponseDto> CreateAsync(CreateEnrollmentDto dto)
        {
            var student = await studentRepository.GetById(dto.StudentId);
            if (student is null)
                throw new NotFoundException(
                    $"Student with ID {dto.StudentId} was not found.");

            var course = await courseRepository.GetById(dto.CourseId);
            if (course is null)
                throw new NotFoundException(
                    $"Course with ID {dto.CourseId} was not found.");

            await EnsureCourseHasCapacityAsync(course);

            var enrollment = new Enrollment
            {
                StudentId = dto.StudentId,
                CourseId = dto.CourseId,
                EnrolledAt = DateTime.UtcNow,
                Status = course.Price == 0
                    ? EnrollmentStatus.Paid
                    : EnrollmentStatus.Pending
            };

            var created = await enrollmentRepository.CreateAsync(enrollment);
            return ToDto(created);
        }

        public async Task<EnrollmentResponseDto> UpdateCourseAsync(
            int id,
            UpdateEnrollmentDto dto)
        {
            var enrollment = await GetEnrollmentOrThrowAsync(id);

            if (enrollment.Status == EnrollmentStatus.Paid &&
                enrollment.CourseId != dto.CourseId)
            {
                throw new ConflictException(
                    "A paid enrollment cannot be moved to another course.");
            }

            var course = await courseRepository.GetById(dto.CourseId);
            if (course is null)
                throw new NotFoundException(
                    $"Course with ID {dto.CourseId} was not found.");

            if (enrollment.CourseId != dto.CourseId)
                await EnsureCourseHasCapacityAsync(course);

            var updated = await enrollmentRepository.UpdateCourseAsync(
                id, dto.CourseId);

            if (updated is null)
                throw new NotFoundException(
                    $"Enrollment with ID {id} was not found.");

            return ToDto(updated);
        }

        public async Task DeleteAsync(int id)
        {
            var enrollment = await GetEnrollmentOrThrowAsync(id);

            if (enrollment.Status == EnrollmentStatus.Paid)
                throw new ConflictException(
                    "A paid enrollment cannot be deleted.");

            if (!await enrollmentRepository.DeleteAsync(id))
                throw new NotFoundException(
                    $"Enrollment with ID {id} was not found.");
        }

        public async Task<List<StudentResponseDto>> GetPaidStudentsForCourseAsync(
            int courseId)
        {
            var course = await courseRepository.GetById(courseId);
            if (course is null)
                throw new NotFoundException(
                    $"Course with ID {courseId} was not found.");

            var students =
                await enrollmentRepository.GetPaidStudentsForCourseAsync(courseId);

            return students.Select(student => student.ToDto()).ToList();
        }

        public async Task<List<StudentCourseDto>> GetCoursesForUserAsync(int userId)
        {
            var student = await enrollmentRepository
                .GetStudentWithCoursesByUserIdAsync(userId);

            if (student is null)
                throw new NotFoundException(
                    "Student profile was not found for the current user.");

            return student.Enrollments
                .Where(e => e.Course is not null)
                .Select(e => new StudentCourseDto(
                    e.CourseId,
                    e.Course!.Title,
                    e.Course.Price,
                    e.EnrolledAt))
                .ToList();
        }

        private async Task EnsureCourseHasCapacityAsync(Course course)
        {
            if (!course.MaxStudents.HasValue) return;

            var paidCount = await enrollmentRepository
                .CountPaidEnrollmentsForCourseAsync(course.Id);

            if (paidCount >= course.MaxStudents.Value)
                throw new ConflictException(
                    "This course has reached its maximum number of paid students.");
        }

        private async Task<Enrollment> GetEnrollmentOrThrowAsync(int id)
        {
            return await enrollmentRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(
                    $"Enrollment with ID {id} was not found.");
        }

        private static EnrollmentResponseDto ToDto(Enrollment enrollment)
        {
            return new EnrollmentResponseDto(
                enrollment.Id,
                enrollment.StudentId,
                enrollment.Student?.FullName ?? string.Empty,
                enrollment.CourseId,
                enrollment.Course?.Title ?? string.Empty,
                enrollment.Course?.Price ?? 0,
                enrollment.EnrolledAt,
                enrollment.Status);
        }
    }
}
