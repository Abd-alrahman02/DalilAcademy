using DalilAcademy.Core.DTOs.Courses;
using FluentValidation;

namespace DalilAcademy.Core.Validators.Courses
{
    public class UpdateCourseDtoValidator : AbstractValidator<UpdateCourseDto>
    {
        public UpdateCourseDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Course title is required.")
                .MaximumLength(200)
                .WithMessage("Course title cannot exceed 200 characters.");

            RuleFor(x => x.Level)
                .NotEmpty()
                .WithMessage("Course level is required.");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Price cannot be negative.");

            RuleFor(x => x.MaxStudents)
                .Must(value => !value.HasValue || value.Value > 0)
                .WithMessage("MaxStudents must be greater than zero when specified.");

            RuleFor(x => x.InstructorId)
                .GreaterThan(0)
                .WithMessage("Instructor ID must be greater than zero.");
        }
    }
}
