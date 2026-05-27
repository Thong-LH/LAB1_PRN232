using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface IStudentRepository
{
    Task<IReadOnlyList<Student>> GetAllAsync(bool includeEnrollments = false);

    Task<Student?> GetByIdAsync(int id, bool includeEnrollments = false);

    Task<bool> ExistsAsync(int id);

    Task<bool> ExistsByEmailAsync(string email, int? excludedStudentId = null);

    Task AddAsync(Student student);

    void Update(Student student);

    void Delete(Student student);

    Task<int> SaveChangesAsync();
}
