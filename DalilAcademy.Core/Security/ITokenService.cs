using DalilAcademy.Core.Models;

namespace DalilAcademy.Core.Security
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) CreateToken(User user);
    }
}