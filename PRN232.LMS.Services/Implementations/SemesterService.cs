using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Mappers;

namespace PRN232.LMS.Services.Implementations;

public class SemesterService : ISemesterService
{
    private readonly ISemesterRepository _semesterRepository;

    public SemesterService(ISemesterRepository semesterRepository)
    {
        _semesterRepository = semesterRepository;
    }

    public async Task<PagedResultBusinessModel<SemesterBusinessModel>> GetAllAsync(CollectionQueryBusinessModel query)
    {
        var includeCourses = query.HasExpand("courses");
        var semesters = await _semesterRepository.GetAllAsync(includeCourses);
        var result = semesters
            .Select(semester => semester.ToBusinessModel(includeCourses))
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            result = result.Where(semester =>
                semester.SemesterName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return ApplySort(result, query.Sort).ToPagedResult(query);
    }

    public async Task<SemesterBusinessModel?> GetByIdAsync(int id)
    {
        var semester = await _semesterRepository.GetByIdAsync(id, includeCourses: true);
        return semester?.ToBusinessModel(includeCourses: true);
    }

    public async Task<SemesterBusinessModel> CreateAsync(SemesterBusinessModel semester)
    {
        ValidateSemester(semester);

        var entity = new Semester
        {
            SemesterName = semester.SemesterName.Trim(),
            StartDate = semester.StartDate,
            EndDate = semester.EndDate
        };

        await _semesterRepository.AddAsync(entity);
        await _semesterRepository.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    public async Task<bool> UpdateAsync(int id, SemesterBusinessModel semester)
    {
        var entity = await _semesterRepository.GetByIdAsync(id);

        if (entity is null)
        {
            return false;
        }

        ValidateSemester(semester);

        entity.SemesterName = semester.SemesterName.Trim();
        entity.StartDate = semester.StartDate;
        entity.EndDate = semester.EndDate;

        _semesterRepository.Update(entity);
        await _semesterRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _semesterRepository.GetByIdAsync(id);

        if (entity is null)
        {
            return false;
        }

        _semesterRepository.Delete(entity);
        await _semesterRepository.SaveChangesAsync();

        return true;
    }

    private static void ValidateSemester(SemesterBusinessModel semester)
    {
        if (string.IsNullOrWhiteSpace(semester.SemesterName))
        {
            throw new ArgumentException("Semester name is required.");
        }

        if (semester.StartDate >= semester.EndDate)
        {
            throw new ArgumentException("Semester start date must be before end date.");
        }
    }

    private static IEnumerable<SemesterBusinessModel> ApplySort(IEnumerable<SemesterBusinessModel> semesters, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return semesters.OrderBy(semester => semester.SemesterId);
        }

        IOrderedEnumerable<SemesterBusinessModel>? orderedSemesters = null;

        foreach (var sortField in sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var descending = sortField.StartsWith('-');
            var field = descending ? sortField[1..] : sortField;
            Func<SemesterBusinessModel, object?> keySelector = field.ToLowerInvariant() switch
            {
                "semesterid" => semester => semester.SemesterId,
                "semestername" => semester => semester.SemesterName,
                "startdate" => semester => semester.StartDate,
                "enddate" => semester => semester.EndDate,
                _ => throw new ArgumentException($"Unknown sort field: {field}.")
            };

            orderedSemesters = orderedSemesters is null
                ? descending ? semesters.OrderByDescending(keySelector) : semesters.OrderBy(keySelector)
                : descending ? orderedSemesters.ThenByDescending(keySelector) : orderedSemesters.ThenBy(keySelector);
        }

        return orderedSemesters ?? semesters;
    }
}
