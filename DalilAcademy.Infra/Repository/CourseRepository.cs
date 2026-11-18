using DalilAcademy.Core.DTOs;
using DalilAcademy.Core.Exceptions;
using DalilAcademy.Core.Models;
using DalilAcademy.Core.Repository;
using DalilAcademy.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DalilAcademy.Infra.Repository
{
    public class CourseRepository(AppDbContext context) : ICourseRepository
    {
        public Task<List<Course>> GetAll()
        {
            return context.Courses
                .AsNoTracking()
                .Include(c => c.Instructor)
                .ToListAsync();
        }

        public Task<Course?> GetById(int id)
        {
            return context.Courses
                .AsNoTracking()
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Course> Create(Course course)
        {
            context.Courses.Add(course);
            await SaveAsync();

            return await context.Courses
                .AsNoTracking()
                .Include(c => c.Instructor)
                .FirstAsync(c => c.Id == course.Id);
        }

        public async Task<Course?> Update(int id, Course updatedCourse)
        {
            var course = await context.Courses
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course is null) return null;

            course.Title = updatedCourse.Title;
            course.Level = updatedCourse.Level;
            course.Price = updatedCourse.Price;
            course.MaxStudents = updatedCourse.MaxStudents;
            course.ImageUrl = updatedCourse.ImageUrl;
            course.InstructorId = updatedCourse.InstructorId;

            await SaveAsync();

            return await context.Courses
                .AsNoTracking()
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> Delete(int id)
        {
            var course = await context.Courses
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course is null) return false;

            context.Courses.Remove(course);
            await SaveAsync();
            return true;
        }

        public async Task<PagedResult<Course>> GetPaged(
            int page,
            int pageSize,
            decimal? minPrice,
            decimal? maxPrice,
            string? search)
        {
            var query = context.Courses
                .AsNoTracking()
                .Include(c => c.Instructor)
                .AsQueryable();

            if (minPrice.HasValue)
                query = query.Where(c => c.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(c => c.Price <= maxPrice.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(c => EF.Functions.ILike(c.Title, $"%{term}%"));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Course>(items, page, pageSize, total);
        }

        public Task<int> Count()
        {
            return context.Courses.CountAsync();
        }

        public Task<bool> ExistsByTitle(string title, int excludeCourseId)
        {
            return context.Courses.AnyAsync(c =>
                c.Title == title && c.Id != excludeCourseId);
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
                    "A course with this title already exists.");
            }
        }
    }
}
