namespace DalilAcademy.Core.DTOs.Auth
{
    public record LoginResponseDto(
        string Token,
        DateTime ExpiresAt
    );
}