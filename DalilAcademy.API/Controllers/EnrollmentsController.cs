using DalilAcademy.Core.DTOs.Enrollments;
using DalilAcademy.Core.DTOs.Students;
using DalilAcademy.Core.Exceptions;
using DalilAcademy.Core.Security;
using DalilAcademy.Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DalilAcademy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrollmentsController(
        IEnrollmentService enrollmentService) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<List<EnrollmentResponseDto>>> GetAll()
        {
            return Ok(await enrollmentService.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<EnrollmentResponseDto>> GetById(int id)
        {
            return Ok(await enrollmentService.GetByIdAsync(id));
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<EnrollmentResponseDto>> Create(
            CreateEnrollmentDto dto)
        {
            var enrollment = await enrollmentService.CreateAsync(dto);
            return CreatedAtAction(
                nameof(GetById),
                new { id = enrollment.Id },
                enrollment);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<EnrollmentResponseDto>> Update(
            int id,
            UpdateEnrollmentDto dto)
        {
            return Ok(await enrollmentService.UpdateCourseAsync(id, dto));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            await enrollmentService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("me")]
        [Authorize(Roles = Roles.Student)]
        public async Task<ActionResult<List<StudentCourseDto>>> MyCourses()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                throw new UnauthorizedException("Invalid user identity.");
            }

            var courses =
                await enrollmentService.GetCoursesForUserAsync(userId);

            return Ok(courses);
        }
    }
}
