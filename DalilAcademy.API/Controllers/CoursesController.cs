using DalilAcademy.Core.DTOs;
using DalilAcademy.Core.DTOs.Courses;
using DalilAcademy.Core.DTOs.Students;
using DalilAcademy.Core.Security;
using DalilAcademy.Core.Service;
using DalilAcademy.Core.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DalilAcademy.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CoursesController(
        ICourseService courseService,
        IImageService images,
        IEnrollmentService enrollmentService) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<CourseResponseDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? search = null)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var courses = await courseService.GetPaged(
                page, pageSize, minPrice, maxPrice, search);

            return Ok(courses);
        }

        [HttpGet("{courseId:int}/students")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<List<StudentResponseDto>>> GetStudents(
            int courseId)
        {
            var students =
                await enrollmentService.GetPaidStudentsForCourseAsync(courseId);

            return Ok(students);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CourseResponseDto>> GetById(int id)
        {
            return Ok(await courseService.GetById(id));
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<CourseResponseDto>> Create(CreateCourseDto dto)
        {
            var course = await courseService.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Instructor}")]
        public async Task<ActionResult<CourseResponseDto>> Update(
            int id,
            UpdateCourseDto dto)
        {
            return Ok(await courseService.Update(id, dto));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            await courseService.Delete(id);
            return NoContent();
        }

        [HttpGet("count")]
        public async Task<ActionResult<int>> Count()
        {
            return Ok(await courseService.Count());
        }

        [HttpPost("upload-image")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file is null)
                return BadRequest("Image file is required.");

            await using var stream = file.OpenReadStream();
            var imageUrl = await images.SaveCourseImageAsync(
                stream, file.FileName, file.Length);

            return Ok(new { imageUrl });
        }
    }
}
