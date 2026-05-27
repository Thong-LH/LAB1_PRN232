using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface IEnrollmentRepository
{
    Task<IReadOnlyList<Enrollment>> GetAllAsync(bool includeStudent = false, bool includeCourse = false);

    Task<IReadOnlyList<Enrollment>> GetByCourseIdAsync(int courseId, bool includeStudent = false, bool includeCourse = false);

    Task<Enrollment?> GetByIdAsync(int id, bool includeStudent = false, bool includeCourse = false);

    Task<bool> ExistsAsync(int id);

    Task AddAsync(Enrollment enrollment);

    void Update(Enrollment enrollment);

    void Delete(Enrollment enrollment);

    Task<int> SaveChangesAsync();
}
