using DalilAcademy.Core.Models;

namespace DalilAcademy.Core.Repository
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
    }
}