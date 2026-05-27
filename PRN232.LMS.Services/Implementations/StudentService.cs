using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Mappers;

namespace PRN232.LMS.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;

    public StudentService(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<PagedResultBusinessModel<StudentBusinessModel>> GetAllAsync(CollectionQueryBusinessModel query)
    {
        var includeEnrollments = query.HasExpand("enrollments");
        var students = await _studentRepository.GetAllAsync(includeEnrollments);
        var result = students
            .Select(student => student.ToBusinessModel(includeEnrollments))
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            result = result.Where(student =>
                student.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                student.Email.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return ApplySort(result, query.Sort).ToPagedResult(query);
    }

    public async Task<StudentBusinessModel?> GetByIdAsync(int id)
    {
        var student = await _studentRepository.GetByIdAsync(id, includeEnrollments: true);
        return student?.ToBusinessModel(includeEnrollments: true);
    }

    public async Task<StudentBusinessModel> CreateAsync(StudentBusinessModel student)
    {
        await ValidateStudentAsync(student);

        var entity = new Student
        {
            FullName = student.FullName.Trim(),
            Email = student.Email.Trim(),
            DateOfBirth = student.DateOfBirth
        };

        await _studentRepository.AddAsync(entity);
        await _studentRepository.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    public async Task<bool> UpdateAsync(int id, StudentBusinessModel student)
    {
        var entity = await _studentRepository.GetByIdAsync(id);

        if (entity is null)
        {
            return false;
        }

        await ValidateStudentAsync(student, id);

        entity.FullName = student.FullName.Trim();
        entity.Email = student.Email.Trim();
        entity.DateOfBirth = student.DateOfBirth;

        _studentRepository.Update(entity);
        await _studentRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _studentRepository.GetByIdAsync(id);

        if (entity is null)
        {
            return false;
        }

        _studentRepository.Delete(entity);
        await _studentRepository.SaveChangesAsync();

        return true;
    }

    private async Task ValidateStudentAsync(StudentBusinessModel student, int? existingStudentId = null)
    {
        if (string.IsNullOrWhiteSpace(student.FullName))
        {
            throw new ArgumentException("Student full name is required.");
        }

        if (string.IsNullOrWhiteSpace(student.Email))
        {
            throw new ArgumentException("Student email is required.");
        }

        if (await _studentRepository.ExistsByEmailAsync(student.Email.Trim(), existingStudentId))
        {
            throw new ArgumentException("Student email already exists.");
        }
    }

    private static IEnumerable<StudentBusinessModel> ApplySort(IEnumerable<StudentBusinessModel> students, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return students.OrderBy(student => student.StudentId);
        }

        IOrderedEnumerable<StudentBusinessModel>? orderedStudents = null;

        foreach (var sortField in sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var descending = sortField.StartsWith('-');
            var field = descending ? sortField[1..] : sortField;
            Func<StudentBusinessModel, object?> keySelector = field.ToLowerInvariant() switch
            {
                "studentid" => student => student.StudentId,
                "fullname" => student => student.FullName,
                "email" => student => student.Email,
                "dateofbirth" => student => student.DateOfBirth,
                _ => throw new ArgumentException($"Unknown sort field: {field}.")
            };

            orderedStudents = orderedStudents is null
                ? descending ? students.OrderByDescending(keySelector) : students.OrderBy(keySelector)
                : descending ? orderedStudents.ThenByDescending(keySelector) : orderedStudents.ThenBy(keySelector);
        }

        return orderedStudents ?? students;
    }
}
