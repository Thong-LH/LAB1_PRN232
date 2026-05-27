using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly LmsDbContext _dbContext;

    public EnrollmentRepository(LmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Enrollment>> GetAllAsync(bool includeStudent = false, bool includeCourse = false)
    {
        return await BuildQuery(includeStudent, includeCourse).ToListAsync();
    }

    public async Task<IReadOnlyList<Enrollment>> GetByCourseIdAsync(int courseId, bool includeStudent = false, bool includeCourse = false)
    {
        return await BuildQuery(includeStudent, includeCourse)
            .Where(enrollment => enrollment.CourseId == courseId)
            .ToListAsync();
    }

    public async Task<Enrollment?> GetByIdAsync(int id, bool includeStudent = false, bool includeCourse = false)
    {
        return await BuildQuery(includeStudent, includeCourse)
            .FirstOrDefaultAsync(enrollment => enrollment.EnrollmentId == id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _dbContext.Enrollments.AnyAsync(enrollment => enrollment.EnrollmentId == id);
    }

    public async Task AddAsync(Enrollment enrollment)
    {
        await _dbContext.Enrollments.AddAsync(enrollment);
    }

    public void Update(Enrollment enrollment)
    {
        _dbContext.Enrollments.Update(enrollment);
    }

    public void Delete(Enrollment enrollment)
    {
        _dbContext.Enrollments.Remove(enrollment);
    }

    public Task<int> SaveChangesAsync()
    {
        return _dbContext.SaveChangesAsync();
    }

    private IQueryable<Enrollment> BuildQuery(bool includeStudent, bool includeCourse)
    {
        var query = _dbContext.Enrollments.AsNoTracking();

        if (includeStudent)
        {
            query = query.Include(enrollment => enrollment.Student);
        }

        if (includeCourse)
        {
            query = query
                .Include(enrollment => enrollment.Course)
                .ThenInclude(course => course!.Semester);
        }

        return query;
    }
}
