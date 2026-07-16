using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.CourseService.Data;
using PRN232.LMS.CourseService.Entities;
using PRN232.LMS.CourseService.Models;
using PRN232.LMS.CourseService.Grpc;
using Asp.Versioning;
using System.Reflection;

namespace PRN232.LMS.CourseService.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/enrollments")]
[Route("api/enrollments")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly CourseDbContext _dbContext;
    private readonly IStudentGrpcClient _studentGrpcClient;
    private readonly ILogger<EnrollmentsController> _logger;

    public EnrollmentsController(CourseDbContext dbContext, IStudentGrpcClient studentGrpcClient, ILogger<EnrollmentsController> logger)
    {
        _dbContext = dbContext;
        _studentGrpcClient = studentGrpcClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResultResponse<object>>>> GetAll([FromQuery] CollectionQueryRequest request)
    {
        var includeStudent = request.Expand != null && request.Expand.Contains("student", StringComparison.OrdinalIgnoreCase);
        var includeCourse = request.Expand != null && request.Expand.Contains("course", StringComparison.OrdinalIgnoreCase);

        var query = _dbContext.Enrollments.AsNoTracking();

        if (includeCourse)
        {
            query = query.Include(e => e.Course);
        }

        query = ApplySort(query, request.Sort);

        var totalItems = await query.CountAsync();
        var page = request.Page;
        var pageSize = request.Size;
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var enrollments = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var responses = new List<EnrollmentResponse>();

        foreach (var e in enrollments)
        {
            StudentSummaryResponse? student = null;
            if (includeStudent)
            {
                // Call StudentService via gRPC to fetch student details
                student = await _studentGrpcClient.GetStudentByIdAsync(e.StudentId);
            }

            responses.Add(new EnrollmentResponse
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                Student = student,
                CourseId = e.CourseId,
                Course = e.Course != null ? new CourseSummaryResponse
                {
                    CourseId = e.Course.CourseId,
                    CourseName = e.Course.CourseName,
                    SemesterId = e.Course.SemesterId
                } : null,
                EnrollDate = e.EnrollDate,
                Status = e.Status
            });
        }

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

        return Ok(ApiResponse<PagedResultResponse<object>>.Ok(result, "Enrollments retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<EnrollmentResponse>>> GetById(int id)
    {
        var e = await _dbContext.Enrollments
            .Include(x => x.Course)
            .FirstOrDefaultAsync(x => x.EnrollmentId == id);

        if (e is null)
        {
            return NotFound(ApiResponse<object>.Fail("Enrollment not found."));
        }

        // Fetch Student via gRPC
        var student = await _studentGrpcClient.GetStudentByIdAsync(e.StudentId);

        var response = new EnrollmentResponse
        {
            EnrollmentId = e.EnrollmentId,
            StudentId = e.StudentId,
            Student = student,
            CourseId = e.CourseId,
            Course = e.Course != null ? new CourseSummaryResponse
            {
                CourseId = e.Course.CourseId,
                CourseName = e.Course.CourseName,
                SemesterId = e.Course.SemesterId
            } : null,
            EnrollDate = e.EnrollDate,
            Status = e.Status
        };

        return Ok(ApiResponse<EnrollmentResponse>.Ok(response, "Enrollment retrieved successfully."));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<EnrollmentResponse>>> Create([FromBody] CreateEnrollmentRequest request)
    {
        _logger.LogInformation("Creating enrollment for Student {StudentId} in Course {CourseId}", request.StudentId, request.CourseId);

        // 1. Verify Student exists via gRPC
        var student = await _studentGrpcClient.GetStudentByIdAsync(request.StudentId);
        if (student is null)
        {
            _logger.LogWarning("Enrollment failed: Student {StudentId} does not exist (verified via gRPC).", request.StudentId);
            return BadRequest(ApiResponse<object>.Fail("Student does not exist."));
        }

        // 2. Verify Course exists
        var course = await _dbContext.Courses.FindAsync(request.CourseId);
        if (course is null)
        {
            return BadRequest(ApiResponse<object>.Fail("Course does not exist."));
        }

        // 3. Perform Enrollment
        var enrollment = new Enrollment
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            EnrollDate = request.EnrollDate,
            Status = request.Status
        };

        await _dbContext.Enrollments.AddAsync(enrollment);
        await _dbContext.SaveChangesAsync();

        var response = new EnrollmentResponse
        {
            EnrollmentId = enrollment.EnrollmentId,
            StudentId = enrollment.StudentId,
            Student = student,
            CourseId = enrollment.CourseId,
            Course = new CourseSummaryResponse
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                SemesterId = course.SemesterId
            },
            EnrollDate = enrollment.EnrollDate,
            Status = enrollment.Status
        };

        return CreatedAtAction(nameof(GetById), new { id = enrollment.EnrollmentId }, ApiResponse<EnrollmentResponse>.Ok(response, "Enrollment created successfully."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Update(int id, [FromBody] UpdateEnrollmentRequest request)
    {
        var enrollment = await _dbContext.Enrollments.FindAsync(id);
        if (enrollment is null)
        {
            return NotFound(ApiResponse<object>.Fail("Enrollment not found."));
        }

        // Verify Student exists via gRPC
        var student = await _studentGrpcClient.GetStudentByIdAsync(request.StudentId);
        if (student is null)
        {
            return BadRequest(ApiResponse<object>.Fail("Student does not exist."));
        }

        // Verify Course exists
        if (!await _dbContext.Courses.AnyAsync(c => c.CourseId == request.CourseId))
        {
            return BadRequest(ApiResponse<object>.Fail("Course does not exist."));
        }

        enrollment.StudentId = request.StudentId;
        enrollment.CourseId = request.CourseId;
        enrollment.EnrollDate = request.EnrollDate;
        enrollment.Status = request.Status;

        _dbContext.Enrollments.Update(enrollment);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "Enrollment updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var enrollment = await _dbContext.Enrollments.FindAsync(id);
        if (enrollment is null)
        {
            return NotFound(ApiResponse<object>.Fail("Enrollment not found."));
        }

        _dbContext.Enrollments.Remove(enrollment);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "Enrollment deleted successfully."));
    }

    private static IQueryable<Enrollment> ApplySort(IQueryable<Enrollment> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query.OrderBy(e => e.EnrollmentId);
        }

        IOrderedQueryable<Enrollment>? orderedQuery = null;

        foreach (var sortField in sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var descending = sortField.StartsWith('-');
            var field = descending ? sortField[1..] : sortField;

            if (field.Equals("enrollmentid", StringComparison.OrdinalIgnoreCase))
            {
                orderedQuery = orderedQuery is null
                    ? descending ? query.OrderByDescending(e => e.EnrollmentId) : query.OrderBy(e => e.EnrollmentId)
                    : descending ? orderedQuery.ThenByDescending(e => e.EnrollmentId) : orderedQuery.ThenBy(e => e.EnrollmentId);
            }
            else if (field.Equals("enrolldate", StringComparison.OrdinalIgnoreCase))
            {
                orderedQuery = orderedQuery is null
                    ? descending ? query.OrderByDescending(e => e.EnrollDate) : query.OrderBy(e => e.EnrollDate)
                    : descending ? orderedQuery.ThenByDescending(e => e.EnrollDate) : orderedQuery.ThenBy(e => e.EnrollDate);
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
