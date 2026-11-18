using DalilAcademy.Core.DTOs.Auth;
using DalilAcademy.Core.Exceptions;
using DalilAcademy.Core.Models;
using DalilAcademy.Core.Repository;
using DalilAcademy.Core.Security;
using DalilAcademy.Core.Service;
using DalilAcademy.Core.Validators.Auth;
using FluentValidation;

namespace DalilAcademy.Infra.Service
{
    public class AuthService(
        IUserRepository userRepository,
        IStudentRepository studentRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IValidator<LoginRequestDto> loginValidator,
        IValidator<RegisterRequestDto> registerValidator
    ) : IAuthService
    {
        public async Task<LoginResponseDto> LoginAsync(
            LoginRequestDto dto)
        {
            await loginValidator.ValidateAndThrowAsync(dto);

            var user = await userRepository.GetByEmailAsync(dto.Email);

            if (user is null ||
                !passwordHasher.Verify(dto.Password, user.PasswordHash))
            {
                throw new UnauthorizedException(
                    "Invalid email or password.");
            }

            var (token, expiresAt) = tokenService.CreateToken(user);

            return new LoginResponseDto(token, expiresAt);
        }

        public async Task<LoginResponseDto> RegisterAsync(
            RegisterRequestDto dto)
        {
            await registerValidator.ValidateAndThrowAsync(dto);

            var existingUser =
                await userRepository.GetByEmailAsync(dto.Email);

            if (existingUser is not null)
            {
                throw new ConflictException(
                    "An account with this email already exists.");
            }

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = passwordHasher.Hash(dto.Password),
                Role = Roles.Student
            };

            var student = new Student
            {
                FullName = dto.FullName,
                Email = dto.Email,
                User = user
            };

            await studentRepository.Create(student);

            var (token, expiresAt) = tokenService.CreateToken(user);

            return new LoginResponseDto(token, expiresAt);
        }
    }
}