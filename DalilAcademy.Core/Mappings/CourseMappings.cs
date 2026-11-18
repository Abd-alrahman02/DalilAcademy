using DalilAcademy.Core.DTOs.Courses;
using DalilAcademy.Core.Models;

namespace DalilAcademy.Core.Mappings
{
    public static class CourseMappings
    {
        public static CourseResponseDto ToDto(this Course course)
        {
            return new CourseResponseDto(
                course.Id,
                course.Title,
                course.Level,
                course.Price,
                course.ImageUrl,
                course.InstructorId,
                course.Instructor?.FullName,
                course.MaxStudents);
        }

        public static Course ToEntity(this CreateCourseDto dto)
        {
            return new Course
            {
                Title = dto.Title,
                Level = dto.Level,
                Price = dto.Price,
                ImageUrl = dto.ImageUrl,
                InstructorId = dto.InstructorId,
                MaxStudents = dto.MaxStudents
            };
        }
    }
}
