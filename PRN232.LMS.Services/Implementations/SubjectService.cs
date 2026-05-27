using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Mappers;

namespace PRN232.LMS.Services.Implementations;

public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _subjectRepository;

    public SubjectService(ISubjectRepository subjectRepository)
    {
        _subjectRepository = subjectRepository;
    }

    public async Task<PagedResultBusinessModel<SubjectBusinessModel>> GetAllAsync(CollectionQueryBusinessModel query)
    {
        var subjects = await _subjectRepository.GetAllAsync();
        var result = subjects
            .Select(subject => subject.ToBusinessModel())
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            result = result.Where(subject =>
                subject.SubjectCode.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                subject.SubjectName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return ApplySort(result, query.Sort).ToPagedResult(query);
    }

    public async Task<SubjectBusinessModel?> GetByIdAsync(int id)
    {
        var subject = await _subjectRepository.GetByIdAsync(id);
        return subject?.ToBusinessModel();
    }

    public async Task<SubjectBusinessModel> CreateAsync(SubjectBusinessModel subject)
    {
        await ValidateSubjectAsync(subject);

        var entity = new Subject
        {
            SubjectCode = subject.SubjectCode.Trim().ToUpperInvariant(),
            SubjectName = subject.SubjectName.Trim(),
            Credit = subject.Credit
        };

        await _subjectRepository.AddAsync(entity);
        await _subjectRepository.SaveChangesAsync();

        return entity.ToBusinessModel();
    }

    public async Task<bool> UpdateAsync(int id, SubjectBusinessModel subject)
    {
        var entity = await _subjectRepository.GetByIdAsync(id);

        if (entity is null)
        {
            return false;
        }

        await ValidateSubjectAsync(subject, id);

        entity.SubjectCode = subject.SubjectCode.Trim().ToUpperInvariant();
        entity.SubjectName = subject.SubjectName.Trim();
        entity.Credit = subject.Credit;

        _subjectRepository.Update(entity);
        await _subjectRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _subjectRepository.GetByIdAsync(id);

        if (entity is null)
        {
            return false;
        }

        _subjectRepository.Delete(entity);
        await _subjectRepository.SaveChangesAsync();

        return true;
    }

    private async Task ValidateSubjectAsync(SubjectBusinessModel subject, int? existingSubjectId = null)
    {
        if (string.IsNullOrWhiteSpace(subject.SubjectCode))
        {
            throw new ArgumentException("Subject code is required.");
        }

        if (string.IsNullOrWhiteSpace(subject.SubjectName))
        {
            throw new ArgumentException("Subject name is required.");
        }

        if (subject.Credit <= 0)
        {
            throw new ArgumentException("Subject credit must be greater than 0.");
        }

        if (await _subjectRepository.ExistsByCodeAsync(subject.SubjectCode.Trim().ToUpperInvariant(), existingSubjectId))
        {
            throw new ArgumentException("Subject code already exists.");
        }
    }

    private static IEnumerable<SubjectBusinessModel> ApplySort(IEnumerable<SubjectBusinessModel> subjects, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return subjects.OrderBy(subject => subject.SubjectId);
        }

        IOrderedEnumerable<SubjectBusinessModel>? orderedSubjects = null;

        foreach (var sortField in sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var descending = sortField.StartsWith('-');
            var field = descending ? sortField[1..] : sortField;
            Func<SubjectBusinessModel, object?> keySelector = field.ToLowerInvariant() switch
            {
                "subjectid" => subject => subject.SubjectId,
                "subjectcode" => subject => subject.SubjectCode,
                "subjectname" => subject => subject.SubjectName,
                "credit" => subject => subject.Credit,
                _ => throw new ArgumentException($"Unknown sort field: {field}.")
            };

            orderedSubjects = orderedSubjects is null
                ? descending ? subjects.OrderByDescending(keySelector) : subjects.OrderBy(keySelector)
                : descending ? orderedSubjects.ThenByDescending(keySelector) : orderedSubjects.ThenBy(keySelector);
        }

        return orderedSubjects ?? subjects;
    }
}
