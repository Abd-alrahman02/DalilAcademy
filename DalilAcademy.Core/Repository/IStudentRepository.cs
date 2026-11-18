using DalilAcademy.Core.DTOs;
using DalilAcademy.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcademy.Core.Repository
{
    public interface IStudentRepository
    {
        Task<List<Student>> GetAll();
        Task<Student?> GetById(int id);
        Task<Student> Create(Student student);
        Task<Student?> Update(int id,Student student);
        Task<bool> Delete(int id);
        Task<Student?> GetWithCourses(int id);
        Task<PagedResult<Student>> GetPaged(int page, int pageSize);

    }
}
