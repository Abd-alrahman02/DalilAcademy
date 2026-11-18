using DalilAcademy.Core.DTOs;
using DalilAcademy.Core.DTOs.Courses;
using DalilAcademy.Core.Exceptions;
using DalilAcademy.Core.Mappings;
using DalilAcademy.Core.Repository;
using DalilAcademy.Core.Service;
using DalilAcademy.Core.Storage;
using FluentValidation;

namespace DalilAcademy.Infra.Service
{
    public class CourseService(
    ICourseRepository courseRepository,
    IValidator<CreateCourseDto> createValidator,
    IValidator<UpdateCourseDto> updateValidator,
    IImageService imageService) : ICourseService
    {
        public async Task<List<CourseResponseDto>> GetAll()
        {
            var courses = await courseRepository.GetAll();

            return courses.Select(course => course.ToDto()).ToList();
        }

        public async Task<CourseResponseDto> GetById(int id)
        {
            var course = await courseRepository.GetById(id);

            if (course is null)
                throw new NotFoundException(
                    $"Course with ID {id} was not found.");

            return course.ToDto();
        }

        public async Task<CourseResponseDto> Create(CreateCourseDto dto)
        {
            await createValidator.ValidateAndThrowAsync(dto);

            var course = dto.ToEntity();

            var createdCourse = await courseRepository.Create(course);

            return createdCourse.ToDto();
        }

        public async Task<CourseResponseDto> Update(
            int id,
            UpdateCourseDto dto)
        {
            await updateValidator.ValidateAndThrowAsync(dto);

            var course = await courseRepository.GetById(id);

            if (course is null)
                throw new NotFoundException(
                    $"Course with ID {id} was not found.");

            var oldImageUrl = course.ImageUrl;

            course.Title = dto.Title;
            course.Level = dto.Level;
            course.Price = dto.Price;
            course.ImageUrl = dto.ImageUrl;
            course.InstructorId = dto.InstructorId;

            if (await courseRepository.ExistsByTitle(dto.Title, id))
            {
                throw new ConflictException(
                    "A course with this title already exists.");
            }

            var updatedCourse = await courseRepository.Update(id, course);

            if (updatedCourse is null)
                throw new NotFoundException(
                    $"Course with ID {id} was not found.");

            if (!string.Equals(
                oldImageUrl,
                updatedCourse.ImageUrl,
                StringComparison.OrdinalIgnoreCase))
            {
                imageService.DeleteCourseImage(oldImageUrl);
            }


            return updatedCourse.ToDto();
        }

        public async Task Delete(int id)
        {
            var course = await courseRepository.GetById(id);

            if (course is null)
                throw new NotFoundException(
                    $"Course with ID {id} was not found.");

            var deleted = await courseRepository.Delete(id);

            if (!deleted)
                throw new NotFoundException(
                    $"Course with ID {id} was not found.");

            imageService.DeleteCourseImage(course.ImageUrl);
        }

        public async Task<PagedResult<CourseResponseDto>> GetPaged(
            int page,
            int pageSize,
            decimal? minPrice,
            decimal? maxPrice)
        {
            var result = await courseRepository.GetPaged(
                page,
                pageSize,
                minPrice,
                maxPrice
            );

            var items = result.Items
                .Select(course => course.ToDto())
                .ToList();

            return new PagedResult<CourseResponseDto>(
                items,
                result.Page,
                result.PageSize,
                result.TotalCount
            );
        }

        public Task<int> Count()
        {
            return courseRepository.Count();
        }
    }
}