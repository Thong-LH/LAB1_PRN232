using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface ISubjectRepository
{
    Task<IReadOnlyList<Subject>> GetAllAsync();

    Task<Subject?> GetByIdAsync(int id);

    Task<bool> ExistsAsync(int id);

    Task<bool> ExistsByCodeAsync(string subjectCode, int? excludedSubjectId = null);

    Task AddAsync(Subject subject);

    void Update(Subject subject);

    void Delete(Subject subject);

    Task<int> SaveChangesAsync();
}
