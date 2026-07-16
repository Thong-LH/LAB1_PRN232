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
[Route("api/v{version:apiVersion}/semesters")]
[Route("api/semesters")]
[Authorize]
public class SemestersController : ControllerBase
{
    private readonly CourseDbContext _dbContext;

    public SemestersController(CourseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResultResponse<object>>>> GetAll([FromQuery] CollectionQueryRequest request)
    {
        var includeCourses = request.Expand != null && request.Expand.Contains("courses", StringComparison.OrdinalIgnoreCase);
        var query = _dbContext.Semesters.AsNoTracking();

        if (includeCourses)
        {
            query = query.Include(s => s.Courses);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(s => s.SemesterName.Contains(search));
        }

        query = ApplySort(query, request.Sort);

        var totalItems = await query.CountAsync();
        var page = request.Page;
        var pageSize = request.Size;
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var semesters = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var responses = semesters.Select(s => new SemesterResponse
        {
            SemesterId = s.SemesterId,
            SemesterName = s.SemesterName,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            Courses = s.Courses?.Select(c => new CourseSummaryResponse
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                SemesterId = c.SemesterId
            }).ToList()
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

        return Ok(ApiResponse<PagedResultResponse<object>>.Ok(result, "Semesters retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<SemesterResponse>>> GetById(int id)
    {
        var semester = await _dbContext.Semesters
            .Include(s => s.Courses)
            .FirstOrDefaultAsync(s => s.SemesterId == id);

        if (semester is null)
        {
            return NotFound(ApiResponse<object>.Fail("Semester not found."));
        }

        var response = new SemesterResponse
        {
            SemesterId = semester.SemesterId,
            SemesterName = semester.SemesterName,
            StartDate = semester.StartDate,
            EndDate = semester.EndDate,
            Courses = semester.Courses?.Select(c => new CourseSummaryResponse
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                SemesterId = c.SemesterId
            }).ToList()
        };

        return Ok(ApiResponse<SemesterResponse>.Ok(response, "Semester retrieved successfully."));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<SemesterResponse>>> Create([FromBody] CreateSemesterRequest request)
    {
        var semester = new Semester
        {
            SemesterName = request.SemesterName.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        await _dbContext.Semesters.AddAsync(semester);
        await _dbContext.SaveChangesAsync();

        var response = new SemesterResponse
        {
            SemesterId = semester.SemesterId,
            SemesterName = semester.SemesterName,
            StartDate = semester.StartDate,
            EndDate = semester.EndDate
        };

        return CreatedAtAction(nameof(GetById), new { id = semester.SemesterId }, ApiResponse<SemesterResponse>.Ok(response, "Semester created successfully."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Update(int id, [FromBody] UpdateSemesterRequest request)
    {
        var semester = await _dbContext.Semesters.FindAsync(id);
        if (semester is null)
        {
            return NotFound(ApiResponse<object>.Fail("Semester not found."));
        }

        semester.SemesterName = request.SemesterName.Trim();
        semester.StartDate = request.StartDate;
        semester.EndDate = request.EndDate;

        _dbContext.Semesters.Update(semester);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "Semester updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var semester = await _dbContext.Semesters.FindAsync(id);
        if (semester is null)
        {
            return NotFound(ApiResponse<object>.Fail("Semester not found."));
        }

        _dbContext.Semesters.Remove(semester);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "Semester deleted successfully."));
    }

    private static IQueryable<Semester> ApplySort(IQueryable<Semester> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query.OrderBy(s => s.SemesterId);
        }

        IOrderedQueryable<Semester>? orderedQuery = null;

        foreach (var sortField in sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var descending = sortField.StartsWith('-');
            var field = descending ? sortField[1..] : sortField;

            if (field.Equals("semesterid", StringComparison.OrdinalIgnoreCase))
            {
                orderedQuery = orderedQuery is null
                    ? descending ? query.OrderByDescending(s => s.SemesterId) : query.OrderBy(s => s.SemesterId)
                    : descending ? orderedQuery.ThenByDescending(s => s.SemesterId) : orderedQuery.ThenBy(s => s.SemesterId);
            }
            else if (field.Equals("semestername", StringComparison.OrdinalIgnoreCase))
            {
                orderedQuery = orderedQuery is null
                    ? descending ? query.OrderByDescending(s => s.SemesterName) : query.OrderBy(s => s.SemesterName)
                    : descending ? orderedQuery.ThenByDescending(s => s.SemesterName) : orderedQuery.ThenBy(s => s.SemesterName);
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
