using DalilAcademy.Core.DTOs.Students;
using FluentValidation;

namespace DalilAcademy.Core.Validators.Students
{
    public class UpdateStudentDtoValidator : AbstractValidator<UpdateStudentDto>
    {
        public UpdateStudentDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty()
                .WithMessage("Full name is required.")
                .MaximumLength(100)
                .WithMessage("Full name cannot exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Invalid email format.")
                .MaximumLength(150)
                .WithMessage("Email cannot exceed 150 characters.");
        }
    }
}