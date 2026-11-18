namespace DalilAcademy.Core.DTOs.Auth
{
    public record RegisterRequestDto(
        string FullName,
        string Email,
        string Password
    );
}