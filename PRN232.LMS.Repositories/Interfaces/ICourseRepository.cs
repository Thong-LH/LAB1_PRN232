using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface ICourseRepository
{
    Task<IReadOnlyList<Course>> GetAllAsync(bool includeSemester = false, bool includeEnrollments = false);

    Task<Course?> GetByIdAsync(int id, bool includeSemester = false, bool includeEnrollments = false);

    Task<bool> ExistsAsync(int id);

    Task AddAsync(Course course);

    void Update(Course course);

    void Delete(Course course);

    Task<int> SaveChangesAsync();
}
