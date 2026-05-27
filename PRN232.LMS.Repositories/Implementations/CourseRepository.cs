using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

public class CourseRepository : ICourseRepository
{
    private readonly LmsDbContext _dbContext;

    public CourseRepository(LmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Course>> GetAllAsync(bool includeSemester = false, bool includeEnrollments = false)
    {
        return await BuildQuery(includeSemester, includeEnrollments).ToListAsync();
    }

    public async Task<Course?> GetByIdAsync(int id, bool includeSemester = false, bool includeEnrollments = false)
    {
        return await BuildQuery(includeSemester, includeEnrollments)
            .FirstOrDefaultAsync(course => course.CourseId == id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _dbContext.Courses.AnyAsync(course => course.CourseId == id);
    }

    public async Task AddAsync(Course course)
    {
        await _dbContext.Courses.AddAsync(course);
    }

    public void Update(Course course)
    {
        _dbContext.Courses.Update(course);
    }

    public void Delete(Course course)
    {
        _dbContext.Courses.Remove(course);
    }

    public Task<int> SaveChangesAsync()
    {
        return _dbContext.SaveChangesAsync();
    }

    private IQueryable<Course> BuildQuery(bool includeSemester, bool includeEnrollments)
    {
        var query = _dbContext.Courses.AsNoTracking();

        if (includeSemester)
        {
            query = query.Include(course => course.Semester);
        }

        if (includeEnrollments)
        {
            query = query
                .Include(course => course.Enrollments)
                .ThenInclude(enrollment => enrollment.Student);
        }

        return query;
    }
}
