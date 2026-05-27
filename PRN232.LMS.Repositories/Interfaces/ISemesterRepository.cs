using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface ISemesterRepository
{
    Task<IReadOnlyList<Semester>> GetAllAsync(bool includeCourses = false);

    Task<Semester?> GetByIdAsync(int id, bool includeCourses = false);

    Task<bool> ExistsAsync(int id);

    Task AddAsync(Semester semester);

    void Update(Semester semester);

    void Delete(Semester semester);

    Task<int> SaveChangesAsync();
}
