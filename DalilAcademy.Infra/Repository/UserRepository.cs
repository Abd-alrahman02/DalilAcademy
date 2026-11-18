using DalilAcademy.Core.Models;
using DalilAcademy.Core.Repository;
using DalilAcademy.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace DalilAcademy.Infra.Repository
{
    public class UserRepository(AppDbContext context)
        : IUserRepository
    {
        public Task<User?> GetByEmailAsync(string email)
        {
            return context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}