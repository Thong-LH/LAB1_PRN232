using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.CourseService.Data;
using PRN232.LMS.CourseService.Entities;
using PRN232.LMS.CourseService.Models;
using Asp.Versioning;
using System.Reflection;

namespace PRN232.LMS.CourseService.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/subjects")]
[Route("api/subjects")]
[Authorize]
public class SubjectsController : ControllerBase
{
    private readonly CourseDbContext _dbContext;

    public SubjectsController(CourseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResultResponse<object>>>> GetAll([FromQuery] CollectionQueryRequest request)
    {
        var query = _dbContext.Subjects.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(s => s.SubjectCode.Contains(search) || s.SubjectName.Contains(search));
        }

        query = ApplySort(query, request.Sort);

        var totalItems = await query.CountAsync();
        var page = request.Page;
        var pageSize = request.Size;
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var subjects = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var responses = subjects.Select(s => new SubjectResponse
        {
            SubjectId = s.SubjectId,
            SubjectCode = s.SubjectCode,
            SubjectName = s.SubjectName,
            Credit = s.Credit
        }).ToList();

        List<object> selectedItems;
        try
        {
            selectedItems = SelectFields(responses, request.Fields);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<PagedResultResponse<object>>.Fail(ex.Message));
        }

        var result = new PagedResultResponse<object>
        {
            Items = selectedItems,
            Pagination = new PaginationMetadata
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            }
        };

        return Ok(ApiResponse<PagedResultResponse<object>>.Ok(result, "Subjects retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<SubjectResponse>>> GetById(int id)
    {
        var subject = await _dbContext.Subjects.FindAsync(id);
        if (subject is null)
        {
            return NotFound(ApiResponse<object>.Fail("Subject not found."));
        }

        var response = new SubjectResponse
        {
            SubjectId = subject.SubjectId,
            SubjectCode = subject.SubjectCode,
            SubjectName = subject.SubjectName,
            Credit = subject.Credit
        };

        return Ok(ApiResponse<SubjectResponse>.Ok(response, "Subject retrieved successfully."));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<SubjectResponse>>> Create([FromBody] CreateSubjectRequest request)
    {
        if (await _dbContext.Subjects.AnyAsync(s => s.SubjectCode == request.SubjectCode.Trim()))
        {
            return BadRequest(ApiResponse<object>.Fail("Subject code already exists."));
        }

        var subject = new Subject
        {
            SubjectCode = request.SubjectCode.Trim(),
            SubjectName = request.SubjectName.Trim(),
            Credit = request.Credit
        };

        await _dbContext.Subjects.AddAsync(subject);
        await _dbContext.SaveChangesAsync();

        var response = new SubjectResponse
        {
            SubjectId = subject.SubjectId,
            SubjectCode = subject.SubjectCode,
            SubjectName = subject.SubjectName,
            Credit = subject.Credit
        };

        return CreatedAtAction(nameof(GetById), new { id = subject.SubjectId }, ApiResponse<SubjectResponse>.Ok(response, "Subject created successfully."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Update(int id, [FromBody] UpdateSubjectRequest request)
    {
        var subject = await _dbContext.Subjects.FindAsync(id);
        if (subject is null)
        {
            return NotFound(ApiResponse<object>.Fail("Subject not found."));
        }

        if (await _dbContext.Subjects.AnyAsync(s => s.SubjectCode == request.SubjectCode.Trim() && s.SubjectId != id))
        {
            return BadRequest(ApiResponse<object>.Fail("Subject code already exists."));
        }

        subject.SubjectCode = request.SubjectCode.Trim();
        subject.SubjectName = request.SubjectName.Trim();
        subject.Credit = request.Credit;

        _dbContext.Subjects.Update(subject);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "Subject updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var subject = await _dbContext.Subjects.FindAsync(id);
        if (subject is null)
        {
            return NotFound(ApiResponse<object>.Fail("Subject not found."));
        }

        _dbContext.Subjects.Remove(subject);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "Subject deleted successfully."));
    }

    private static IQueryable<Subject> ApplySort(IQueryable<Subject> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query.OrderBy(s => s.SubjectId);
        }

        IOrderedQueryable<Subject>? orderedQuery = null;

        foreach (var sortField in sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var descending = sortField.StartsWith('-');
            var field = descending ? sortField[1..] : sortField;

            if (field.Equals("subjectid", StringComparison.OrdinalIgnoreCase))
            {
                orderedQuery = orderedQuery is null
                    ? descending ? query.OrderByDescending(s => s.SubjectId) : query.OrderBy(s => s.SubjectId)
                    : descending ? orderedQuery.ThenByDescending(s => s.SubjectId) : orderedQuery.ThenBy(s => s.SubjectId);
            }
            else if (field.Equals("subjectcode", StringComparison.OrdinalIgnoreCase))
            {
                orderedQuery = orderedQuery is null
                    ? descending ? query.OrderByDescending(s => s.SubjectCode) : query.OrderBy(s => s.SubjectCode)
                    : descending ? orderedQuery.ThenByDescending(s => s.SubjectCode) : orderedQuery.ThenBy(s => s.SubjectCode);
            }
            else if (field.Equals("subjectname", StringComparison.OrdinalIgnoreCase))
            {
                orderedQuery = orderedQuery is null
                    ? descending ? query.OrderByDescending(s => s.SubjectName) : query.OrderBy(s => s.SubjectName)
                    : descending ? orderedQuery.ThenByDescending(s => s.SubjectName) : orderedQuery.ThenBy(s => s.SubjectName);
            }
        }

        return orderedQuery ?? query;
    }

    private static List<object> SelectFields<T>(IEnumerable<T> items, string? fields) where T : class
    {
        if (string.IsNullOrWhiteSpace(fields))
        {
            return items.Cast<object>().ToList();
        }

        var requestedFields = fields
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (requestedFields.Count == 0)
        {
            return items.Cast<object>().ToList();
        }

        var properties = typeof(T)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .ToDictionary(property => property.Name, StringComparer.OrdinalIgnoreCase);

        var unknownField = requestedFields.FirstOrDefault(field => !properties.ContainsKey(field));
        if (unknownField is not null)
        {
            throw new ArgumentException($"Unknown field: {unknownField}.");
        }

        return items
            .Select(item => (object)requestedFields.ToDictionary(
                field => field,
                field => properties[field].GetValue(item)))
            .ToList();
    }
}
