using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Mappers;

namespace PRN232.LMS.Services.Implementations;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseRepository _courseRepository;

    public EnrollmentService(
        IEnrollmentRepository enrollmentRepository,
        IStudentRepository studentRepository,
        ICourseRepository courseRepository)
    {
        _enrollmentRepository = enrollmentRepository;
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
    }

    public async Task<PagedResultBusinessModel<EnrollmentBusinessModel>> GetAllAsync(CollectionQueryBusinessModel query)
    {
        var includeStudent = query.HasExpand("student");
        var includeCourse = query.HasExpand("course");
        var enrollments = await _enrollmentRepository.GetAllAsync(includeStudent, includeCourse);
        var result = enrollments
            .Select(enrollment => enrollment.ToBusinessModel(includeStudent: true, includeCourse: true))
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            result = result.Where(enrollment =>
                enrollment.Status.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (enrollment.Student?.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (enrollment.Student?.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (enrollment.Course?.CourseName.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return ApplySort(result, query.Sort).ToPagedResult(query);
    }

    public async Task<PagedResultBusinessModel<EnrollmentBusinessModel>?> GetByCourseIdAsync(int courseId, CollectionQueryBusinessModel query)
    {
        if (!await _courseRepository.ExistsAsync(courseId))
        {
            return null;
        }

        var includeStudent = query.HasExpand("student");
        var includeCourse = query.HasExpand("course");
        var enrollments = await _enrollmentRepository.GetByCourseIdAsync(courseId, includeStudent, includeCourse);
        var result = enrollments
            .Select(enrollment => enrollment.ToBusinessModel(includeStudent, includeCourse))
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            result = result.Where(enrollment =>
                enrollment.Status.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (enrollment.Student?.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (enrollment.Student?.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (enrollment.Course?.CourseName.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return ApplySort(result, query.Sort).ToPagedResult(query);
    }

    public async Task<EnrollmentBusinessModel?> GetByIdAsync(int id)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(id, includeStudent: true, includeCourse: true);
        return enrollment?.ToBusinessModel(includeStudent: true, includeCourse: true);
    }

    public async Task<EnrollmentBusinessModel> CreateAsync(EnrollmentBusinessModel enrollment)
    {
        await ValidateEnrollmentAsync(enrollment);

        var entity = new Enrollment
        {
            StudentId = enrollment.StudentId,
            CourseId = enrollment.CourseId,
            EnrollDate = enrollment.EnrollDate,
            Status = enrollment.Status.Trim()
        };

        await _enrollmentRepository.AddAsync(entity);
        await _enrollmentRepository.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    public async Task<bool> UpdateAsync(int id, EnrollmentBusinessModel enrollment)
    {
        var entity = await _enrollmentRepository.GetByIdAsync(id);

        if (entity is null)
        {
            return false;
        }

        await ValidateEnrollmentAsync(enrollment);

        entity.StudentId = enrollment.StudentId;
        entity.CourseId = enrollment.CourseId;
        entity.EnrollDate = enrollment.EnrollDate;
        entity.Status = enrollment.Status.Trim();

        _enrollmentRepository.Update(entity);
        await _enrollmentRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _enrollmentRepository.GetByIdAsync(id);

        if (entity is null)
        {
            return false;
        }

        _enrollmentRepository.Delete(entity);
        await _enrollmentRepository.SaveChangesAsync();

        return true;
    }

    private async Task ValidateEnrollmentAsync(EnrollmentBusinessModel enrollment)
    {
        if (!await _studentRepository.ExistsAsync(enrollment.StudentId))
        {
            throw new ArgumentException("Student does not exist.");
        }

        if (!await _courseRepository.ExistsAsync(enrollment.CourseId))
        {
            throw new ArgumentException("Course does not exist.");
        }

        if (string.IsNullOrWhiteSpace(enrollment.Status))
        {
            throw new ArgumentException("Enrollment status is required.");
        }
    }

    private static IEnumerable<EnrollmentBusinessModel> ApplySort(IEnumerable<EnrollmentBusinessModel> enrollments, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return enrollments.OrderBy(enrollment => enrollment.EnrollmentId);
        }

        IOrderedEnumerable<EnrollmentBusinessModel>? orderedEnrollments = null;

        foreach (var sortField in sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var descending = sortField.StartsWith('-');
            var field = descending ? sortField[1..] : sortField;
            Func<EnrollmentBusinessModel, object?> keySelector = field.ToLowerInvariant() switch
            {
                "enrollmentid" => enrollment => enrollment.EnrollmentId,
                "studentid" => enrollment => enrollment.StudentId,
                "courseid" => enrollment => enrollment.CourseId,
                "enrolldate" => enrollment => enrollment.EnrollDate,
                "status" => enrollment => enrollment.Status,
                "studentname" => enrollment => enrollment.Student?.FullName,
                "coursename" => enrollment => enrollment.Course?.CourseName,
                _ => throw new ArgumentException($"Unknown sort field: {field}.")
            };

            orderedEnrollments = orderedEnrollments is null
                ? descending ? enrollments.OrderByDescending(keySelector) : enrollments.OrderBy(keySelector)
                : descending ? orderedEnrollments.ThenByDescending(keySelector) : orderedEnrollments.ThenBy(keySelector);
        }

        return orderedEnrollments ?? enrollments;
    }
}
