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
    public class EnrollmentsController(
        IEnrollmentService enrollmentService) : ControllerBase
    {
        [HttpGet("me")]
        [Authorize(Roles = Roles.Student)]
        public async Task<ActionResult<List<StudentCourseDto>>> MyCourses()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                throw new UnauthorizedException(
                    "Invalid user identity.");
            }

            var courses =
                await enrollmentService.GetCoursesForUserAsync(userId);

            return Ok(courses);
        }
    }
}