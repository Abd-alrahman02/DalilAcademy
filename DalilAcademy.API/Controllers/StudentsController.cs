
using DalilAcademy.Core.DTOs;
using DalilAcademy.Core.DTOs.Students;
using DalilAcademy.Core.Security;
using DalilAcademy.Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DalilAcademy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.Admin)]
    public class StudentsController(IStudentService studentService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<PagedResult<StudentResponseDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var students = await studentService.GetPaged(
                page,
                pageSize
            );

            return Ok(students);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<StudentResponseDto>> GetById(int id)
        {
            var student = await studentService.GetById(id);

            return Ok(student);
        }

        [HttpPost]
        public async Task<ActionResult<StudentResponseDto>> Create(CreateStudentDto dto)
        {
            var student = await studentService.Create(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = student.Id },
                student
            );
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<StudentResponseDto>> Update(int id,UpdateStudentDto dto)
        {
            var student = await studentService.Update(id, dto);

            return Ok(student);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await studentService.Delete(id);

            return NoContent();
        }

        [HttpGet("{id:int}/courses")]
        public async Task<ActionResult<List<StudentCourseDto>>> GetCourses(int id)
        {
            var courses = await studentService.GetCourses(id);

            return Ok(courses);
        }
    }
}
