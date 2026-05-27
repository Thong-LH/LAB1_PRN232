using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Mappers;

namespace PRN232.LMS.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly ISemesterRepository _semesterRepository;

    public CourseService(ICourseRepository courseRepository, ISemesterRepository semesterRepository)
    {
        _courseRepository = courseRepository;
        _semesterRepository = semesterRepository;
    }

    public async Task<PagedResultBusinessModel<CourseBusinessModel>> GetAllAsync(CollectionQueryBusinessModel query)
    {
        var includeSemester = query.HasExpand("semester");
        var includeEnrollments = query.HasExpand("enrollments");
        var courses = await _courseRepository.GetAllAsync(includeSemester, includeEnrollments);
        var result = courses
            .Select(course => course.ToBusinessModel(includeSemester, includeEnrollments))
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            result = result.Where(course =>
                course.CourseName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (course.Semester?.SemesterName.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return ApplySort(result, query.Sort).ToPagedResult(query);
    }

    public async Task<CourseBusinessModel?> GetByIdAsync(int id)
    {
        var course = await _courseRepository.GetByIdAsync(id, includeSemester: true, includeEnrollments: true);
        return course?.ToBusinessModel(includeSemester: true, includeEnrollments: true);
    }

    public async Task<CourseBusinessModel> CreateAsync(CourseBusinessModel course)
    {
        await ValidateCourseAsync(course);

        var entity = new Course
        {
            CourseName = course.CourseName.Trim(),
            SemesterId = course.SemesterId
        };

        await _courseRepository.AddAsync(entity);
        await _courseRepository.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    public async Task<bool> UpdateAsync(int id, CourseBusinessModel course)
    {
        var entity = await _courseRepository.GetByIdAsync(id);

        if (entity is null)
        {
            return false;
        }

        await ValidateCourseAsync(course);

        entity.CourseName = course.CourseName.Trim();
        entity.SemesterId = course.SemesterId;

        _courseRepository.Update(entity);
        await _courseRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _courseRepository.GetByIdAsync(id);

        if (entity is null)
        {
            return false;
        }

        _courseRepository.Delete(entity);
        await _courseRepository.SaveChangesAsync();

        return true;
    }

    private async Task ValidateCourseAsync(CourseBusinessModel course)
    {
        if (string.IsNullOrWhiteSpace(course.CourseName))
        {
            throw new ArgumentException("Course name is required.");
        }

        if (!await _semesterRepository.ExistsAsync(course.SemesterId))
        {
            throw new ArgumentException("Semester does not exist.");
        }
    }

    private static IEnumerable<CourseBusinessModel> ApplySort(IEnumerable<CourseBusinessModel> courses, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return courses.OrderBy(course => course.CourseId);
        }

        IOrderedEnumerable<CourseBusinessModel>? orderedCourses = null;

        foreach (var sortField in sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var descending = sortField.StartsWith('-');
            var field = descending ? sortField[1..] : sortField;
            Func<CourseBusinessModel, object?> keySelector = field.ToLowerInvariant() switch
            {
                "courseid" => course => course.CourseId,
                "coursename" => course => course.CourseName,
                "semesterid" => course => course.SemesterId,
                "semestername" => course => course.Semester?.SemesterName,
                _ => throw new ArgumentException($"Unknown sort field: {field}.")
            };

            orderedCourses = orderedCourses is null
                ? descending ? courses.OrderByDescending(keySelector) : courses.OrderBy(keySelector)
                : descending ? orderedCourses.ThenByDescending(keySelector) : orderedCourses.ThenBy(keySelector);
        }

        return orderedCourses ?? courses;
    }
}
