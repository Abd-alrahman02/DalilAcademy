using DalilAcademy.Core.DTOs;
using DalilAcademy.Core.Exceptions;
using DalilAcademy.Core.Models;
using DalilAcademy.Core.Repository;
using DalilAcademy.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DalilAcademy.Infra.Repository
{
    public class CourseRepository : ICourseRepository
    {
        private readonly AppDbContext _context;

        public CourseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Course>> GetAll()
        {
            return await _context.Courses
                .AsNoTracking()
                .Include(c => c.Instructor)
                .ToListAsync();
        }

        public async Task<Course?> GetById(int id)
        {
            return await _context.Courses
                .AsNoTracking()
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Course> Create(Course course)
        {
            _context.Courses.Add(course);
            await Save();

            return course;
        }

        public async Task<Course?> Update(int id, Course updatedCourse)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course is null)
                return null;

            course.Title = updatedCourse.Title;
            course.Level = updatedCourse.Level;
            course.Price = updatedCourse.Price;
            course.InstructorId = updatedCourse.InstructorId;

            await Save();

            return course;
        }

        public async Task<bool> Delete(int id)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course is null)
                return false;

            _context.Courses.Remove(course);
            await Save();

            return true;
        }

        

        public async Task<PagedResult<Course>> GetPaged(
            int page,
            int pageSize,
            decimal? minPrice,
            decimal? maxPrice)
        {
            var query = _context.Courses
                .AsNoTracking()
                .Include(c => c.Instructor)
                .AsQueryable();

            if (minPrice.HasValue)
                query = query.Where(c => c.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(c => c.Price <= maxPrice.Value);

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            return new PagedResult<Course>(
                items,
                page,
                pageSize,
                total
            );
        }

        public async Task<int> Count()
        {
            return await _context.Courses.CountAsync();
        }

        public async Task<bool> ExistsByTitle(string title)
        {
            return await _context.Courses
                .AnyAsync(c => c.Title == title);
        }

        private async Task Save()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation
                })
            {
                throw new ConflictException("A course with this title already exists.");
            }
        }
    }
}