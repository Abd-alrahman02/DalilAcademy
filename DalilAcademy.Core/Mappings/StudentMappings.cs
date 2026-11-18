using DalilAcademy.Core.DTOs.Students;
using DalilAcademy.Core.Models;

namespace DalilAcademy.Core.Mappings
{
    public static class StudentMappings
    {
        public static StudentResponseDto ToDto(this Student student)
        {
            return new StudentResponseDto(
                student.Id,
                student.FullName,
                student.Email
            );
        }

        public static Student ToEntity(this CreateStudentDto dto)
        {
            return new Student
            {
                FullName = dto.FullName,
                Email = dto.Email
            };
        }
    }
}
