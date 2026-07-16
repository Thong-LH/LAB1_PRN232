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
[Route("api/v{version:apiVersion}/courses")]
[Route("api/courses")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly CourseDbContext _dbContext;

    public CoursesController(CourseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResultResponse<object>>>> GetAll([FromQuery] CollectionQueryRequest request)
    {
        var includeSemester = request.Expand != null && request.Expand.Contains("semester", StringComparison.OrdinalIgnoreCase);
        var includeEnrollments = request.Expand != null && request.Expand.Contains("enrollments", StringComparison.OrdinalIgnoreCase);

        var query = _dbContext.Courses.AsNoTracking();

        if (includeSemester)
        {
            query = query.Include(c => c.Semester);
        }

        if (includeEnrollments)
        {
            query = query.Include(c => c.Enrollments);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(c => c.CourseName.Contains(search));
        }

        query = ApplySort(query, request.Sort);

        var totalItems = await query.CountAsync();
        var page = request.Page;
        var pageSize = request.Size;
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var courses = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var responses = courses.Select(c => new CourseResponse
        {
            CourseId = c.CourseId,
            CourseName = c.CourseName,
            SemesterId = c.SemesterId,
            Semester = c.Semester != null ? new SemesterSummaryResponse
            {
                SemesterId = c.Semester.SemesterId,
                SemesterName = c.Semester.SemesterName
            } : null,
            Enrollments = c.Enrollments?.Select(e => new EnrollmentSummaryResponse
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                CourseId = e.CourseId,
                EnrollDate = e.EnrollDate,
                Status = e.Status
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

        return Ok(ApiResponse<PagedResultResponse<object>>.Ok(result, "Courses retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CourseResponse>>> GetById(int id)
    {
        var course = await _dbContext.Courses
            .Include(c => c.Semester)
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.CourseId == id);

        if (course is null)
        {
            return NotFound(ApiResponse<object>.Fail("Course not found."));
        }

        var response = new CourseResponse
        {
            CourseId = course.CourseId,
            CourseName = course.CourseName,
            SemesterId = course.SemesterId,
            Semester = course.Semester != null ? new SemesterSummaryResponse
            {
                SemesterId = course.Semester.SemesterId,
                SemesterName = course.Semester.SemesterName
            } : null,
            Enrollments = course.Enrollments?.Select(e => new EnrollmentSummaryResponse
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                CourseId = e.CourseId,
                EnrollDate = e.EnrollDate,
                Status = e.Status
            }).ToList()
        };

        return Ok(ApiResponse<CourseResponse>.Ok(response, "Course retrieved successfully."));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<CourseResponse>>> Create([FromBody] CreateCourseRequest request)
    {
        if (!await _dbContext.Semesters.AnyAsync(s => s.SemesterId == request.SemesterId))
        {
            return BadRequest(ApiResponse<object>.Fail("Semester ID does not exist."));
        }

        var course = new Course
        {
            CourseName = request.CourseName.Trim(),
            SemesterId = request.SemesterId
        };

        await _dbContext.Courses.AddAsync(course);
        await _dbContext.SaveChangesAsync();

        var response = new CourseResponse
        {
            CourseId = course.CourseId,
            CourseName = course.CourseName,
            SemesterId = course.SemesterId
        };

        return CreatedAtAction(nameof(GetById), new { id = course.CourseId }, ApiResponse<CourseResponse>.Ok(response, "Course created successfully."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Update(int id, [FromBody] UpdateCourseRequest request)
    {
        var course = await _dbContext.Courses.FindAsync(id);
        if (course is null)
        {
            return NotFound(ApiResponse<object>.Fail("Course not found."));
        }

        if (!await _dbContext.Semesters.AnyAsync(s => s.SemesterId == request.SemesterId))
        {
            return BadRequest(ApiResponse<object>.Fail("Semester ID does not exist."));
        }

        course.CourseName = request.CourseName.Trim();
        course.SemesterId = request.SemesterId;

        _dbContext.Courses.Update(course);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "Course updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var course = await _dbContext.Courses.FindAsync(id);
        if (course is null)
        {
            return NotFound(ApiResponse<object>.Fail("Course not found."));
        }

        _dbContext.Courses.Remove(course);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "Course deleted successfully."));
    }

    private static IQueryable<Course> ApplySort(IQueryable<Course> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query.OrderBy(c => c.CourseId);
        }

        IOrderedQueryable<Course>? orderedQuery = null;

        foreach (var sortField in sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var descending = sortField.StartsWith('-');
            var field = descending ? sortField[1..] : sortField;

            if (field.Equals("courseid", StringComparison.OrdinalIgnoreCase))
            {
                orderedQuery = orderedQuery is null
                    ? descending ? query.OrderByDescending(c => c.CourseId) : query.OrderBy(c => c.CourseId)
                    : descending ? orderedQuery.ThenByDescending(c => c.CourseId) : orderedQuery.ThenBy(c => c.CourseId);
            }
            else if (field.Equals("coursename", StringComparison.OrdinalIgnoreCase))
            {
                orderedQuery = orderedQuery is null
                    ? descending ? query.OrderByDescending(c => c.CourseName) : query.OrderBy(c => c.CourseName)
                    : descending ? orderedQuery.ThenByDescending(c => c.CourseName) : orderedQuery.ThenBy(c => c.CourseName);
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
