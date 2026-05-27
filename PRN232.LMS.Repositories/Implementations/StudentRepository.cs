using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

public class StudentRepository : IStudentRepository
{
    private readonly LmsDbContext _dbContext;

    public StudentRepository(LmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Student>> GetAllAsync(bool includeEnrollments = false)
    {
        return await BuildQuery(includeEnrollments).ToListAsync();
    }

    public async Task<Student?> GetByIdAsync(int id, bool includeEnrollments = false)
    {
        return await BuildQuery(includeEnrollments)
            .FirstOrDefaultAsync(student => student.StudentId == id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _dbContext.Students.AnyAsync(student => student.StudentId == id);
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludedStudentId = null)
    {
        return await _dbContext.Students.AnyAsync(student =>
            student.Email == email &&
            (!excludedStudentId.HasValue || student.StudentId != excludedStudentId.Value));
    }

    public async Task AddAsync(Student student)
    {
        await _dbContext.Students.AddAsync(student);
    }

    public void Update(Student student)
    {
        _dbContext.Students.Update(student);
    }

    public void Delete(Student student)
    {
        _dbContext.Students.Remove(student);
    }

    public Task<int> SaveChangesAsync()
    {
        return _dbContext.SaveChangesAsync();
    }

    private IQueryable<Student> BuildQuery(bool includeEnrollments)
    {
        var query = _dbContext.Students.AsNoTracking();

        if (includeEnrollments)
        {
            query = query
                .Include(student => student.Enrollments)
                .ThenInclude(enrollment => enrollment.Course);
        }

        return query;
    }
}
