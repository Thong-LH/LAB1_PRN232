using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

public class SemesterRepository : ISemesterRepository
{
    private readonly LmsDbContext _dbContext;

    public SemesterRepository(LmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Semester>> GetAllAsync(bool includeCourses = false)
    {
        return await BuildQuery(includeCourses).ToListAsync();
    }

    public async Task<Semester?> GetByIdAsync(int id, bool includeCourses = false)
    {
        return await BuildQuery(includeCourses)
            .FirstOrDefaultAsync(semester => semester.SemesterId == id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _dbContext.Semesters.AnyAsync(semester => semester.SemesterId == id);
    }

    public async Task AddAsync(Semester semester)
    {
        await _dbContext.Semesters.AddAsync(semester);
    }

    public void Update(Semester semester)
    {
        _dbContext.Semesters.Update(semester);
    }

    public void Delete(Semester semester)
    {
        _dbContext.Semesters.Remove(semester);
    }

    public Task<int> SaveChangesAsync()
    {
        return _dbContext.SaveChangesAsync();
    }

    private IQueryable<Semester> BuildQuery(bool includeCourses)
    {
        var query = _dbContext.Semesters.AsNoTracking();

        if (includeCourses)
        {
            query = query.Include(semester => semester.Courses);
        }

        return query;
    }
}
