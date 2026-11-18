
using DalilAcademy.Core.DTOs;
using DalilAcademy.Core.DTOs.Students;
using DalilAcademy.Core.Exceptions;
using DalilAcademy.Core.Mappings;
using DalilAcademy.Core.Repository;
using DalilAcademy.Core.Service;
using FluentValidation;

namespace DalilAcademy.Infra.Service
{
    public class StudentService(
        IStudentRepository studentRepository,
        IValidator<CreateStudentDto> createValidator,
        IValidator<UpdateStudentDto> updateValidator) : IStudentService
    {
        public async Task<List<StudentResponseDto>> GetAll()
        {
            var students = await studentRepository.GetAll();

            return students.Select(student => student.ToDto()).ToList();
        }

        public async Task<StudentResponseDto> GetById(int id)
        {
            var student = await studentRepository.GetById(id);

            if (student is null)
                throw new NotFoundException($"Student with ID {id} was not found.");

            return student.ToDto();
        }

        public async Task<StudentResponseDto> Create(CreateStudentDto dto)
        {
            await createValidator.ValidateAndThrowAsync(dto);

            var student = dto.ToEntity();

            var createdStudent = await studentRepository.Create(student);

            return createdStudent.ToDto();
        }

        public async Task<StudentResponseDto> Update(int id, UpdateStudentDto dto)
        {
            await updateValidator.ValidateAndThrowAsync(dto);

            var student = await studentRepository.GetById(id);

            if (student is null)
                throw new NotFoundException($"Student with ID {id} was not found.");

            student.FullName = dto.FullName;
            student.Email = dto.Email;

            var updatedStudent = await studentRepository.Update(id, student);

            if (updatedStudent is null)
                throw new NotFoundException($"Student with ID {id} was not found.");

            return updatedStudent.ToDto();
        }

        public async Task Delete(int id)
        {
            var deleted = await studentRepository.Delete(id);

            if (!deleted)
                throw new NotFoundException($"Student with ID {id} was not found.");
        }

        public async Task<List<StudentCourseDto>> GetCourses(int id)
        {
            var student = await studentRepository.GetWithCourses(id);

            if (student is null)
                throw new NotFoundException(
                    $"Student with ID {id} was not found.");

            return student.Enrollments
                .Select(e => new StudentCourseDto(
                    e.CourseId,
                    e.Course!.Title,
                    e.Course.Price,
                    e.EnrolledAt
                ))
                .ToList();
        }
        public async Task<PagedResult<StudentResponseDto>> GetPaged(
            int page,
            int pageSize)
        {
            var result = await studentRepository.GetPaged(
                page,
                pageSize
            );

            var items = result.Items
                .Select(student => student.ToDto())
                .ToList();

            return new PagedResult<StudentResponseDto>(
                items,
                result.Page,
                result.PageSize,
                result.TotalCount
            );
        }
    }
}
