
using DalilAcademy.Core.DTOs;
using DalilAcademy.Core.DTOs.Students;

namespace DalilAcademy.Core.Service
{
    public interface IStudentService
    {
        Task<List<StudentResponseDto>> GetAll();

        Task<StudentResponseDto> GetById(int id);

        Task<StudentResponseDto> Create(CreateStudentDto dto);

        Task<StudentResponseDto> Update(int id, UpdateStudentDto dto);

        Task Delete(int id);
        Task<List<StudentCourseDto>> GetCourses(int id);

        Task<PagedResult<StudentResponseDto>> GetPaged(
            int page,
            int pageSize
        );
    }
}
