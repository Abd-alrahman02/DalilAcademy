using DalilAcademy.Core.DTOs;
using DalilAcademy.Core.Exceptions;
using DalilAcademy.Core.Models;
using DalilAcademy.Core.Repository;
using DalilAcademy.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DalilAcademy.Infra.Repository
{
    public class StudentRepository : IStudentRepository
    {
        private readonly AppDbContext _context;

        public StudentRepository(AppDbContext context)
        {
            _context = context;
        }

        //GET ALL
        public async Task<List<Student>> GetAll()
        {
            return await _context.Students.ToListAsync();
        }

        //GET BY ID 
        public async Task<Student?> GetById(int id)
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.Id == id);
        }

        //CREATE
        public async Task<Student> Create(Student student)
        {
            _context.Students.Add(student);
            await Save();

            return student;
        }

        //UPDATE
        public async Task<Student?> Update(int id, Student updatedStudent)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s =>s.Id == id);

            if(student == null)
            {
                return null;
            }

            student.FullName = updatedStudent.FullName;
            student.Email = updatedStudent.Email;

            await Save();
            return student;
        }

        //DELETE
        public async Task<bool> Delete(int id)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return false;
            }

            _context.Students.Remove(student);
            await Save();

            return true;
        }

        public async Task<Student?> GetWithCourses(int id)
        {
            return await _context.Students
                .AsNoTracking()
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<PagedResult<Student>> GetPaged(
            int page,
            int pageSize)
        {
            var query = _context.Students.AsNoTracking();

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(s => s.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Student>(
                items,
                page,
                pageSize,
                total
            );
        }



        //SAVE CHANGES
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
                throw new ConflictException("A student with this email already exists.");
            }
        }
    }
}
