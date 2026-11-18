using DalilAcademy.Core.DTOs;
using DalilAcademy.Core.DTOs.Courses;

namespace DalilAcademy.Core.Service
{
    public interface ICourseService
    {
        Task<List<CourseResponseDto>> GetAll();

        Task<CourseResponseDto> GetById(int id);

        Task<CourseResponseDto> Create(CreateCourseDto dto);

        Task<CourseResponseDto> Update(int id, UpdateCourseDto dto);

        Task Delete(int id);

        Task<PagedResult<CourseResponseDto>> GetPaged(
            int page,
            int pageSize,
            decimal? minPrice,
            decimal? maxPrice
        );

        Task<int> Count();
    }
}