using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

public class SubjectRepository : ISubjectRepository
{
    private readonly LmsDbContext _dbContext;

    public SubjectRepository(LmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Subject>> GetAllAsync()
    {
        return await _dbContext.Subjects.AsNoTracking().ToListAsync();
    }

    public async Task<Subject?> GetByIdAsync(int id)
    {
        return await _dbContext.Subjects
            .AsNoTracking()
            .FirstOrDefaultAsync(subject => subject.SubjectId == id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _dbContext.Subjects.AnyAsync(subject => subject.SubjectId == id);
    }

    public async Task<bool> ExistsByCodeAsync(string subjectCode, int? excludedSubjectId = null)
    {
        return await _dbContext.Subjects.AnyAsync(subject =>
            subject.SubjectCode == subjectCode &&
            (!excludedSubjectId.HasValue || subject.SubjectId != excludedSubjectId.Value));
    }

    public async Task AddAsync(Subject subject)
    {
        await _dbContext.Subjects.AddAsync(subject);
    }

    public void Update(Subject subject)
    {
        _dbContext.Subjects.Update(subject);
    }

    public void Delete(Subject subject)
    {
        _dbContext.Subjects.Remove(subject);
    }

    public Task<int> SaveChangesAsync()
    {
        return _dbContext.SaveChangesAsync();
    }
}
