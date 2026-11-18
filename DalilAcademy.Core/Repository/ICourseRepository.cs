using DalilAcademy.Core.DTOs;
using DalilAcademy.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcademy.Core.Repository
{
    public interface ICourseRepository
    {
        Task<List<Course>> GetAll();
        Task<Course?> GetById(int id);
        Task<Course> Create(Course course);
        Task<Course?> Update(int id, Course updatedCourse);
        Task<bool> Delete(int id);
        Task<PagedResult<Course>> GetPaged(
            int page,
            int pageSize,
            decimal? minPrice,
            decimal? maxPrice
        );
        Task<int> Count();
        Task<bool> ExistsByTitle(string title, int excludeCourseId);
    }
}
