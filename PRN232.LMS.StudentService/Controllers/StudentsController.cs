using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.StudentService.Data;
using PRN232.LMS.StudentService.Entities;
using PRN232.LMS.StudentService.Models;
using Asp.Versioning;
using System.Reflection;

namespace PRN232.LMS.StudentService.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/students")]
[Route("api/students")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly StudentDbContext _dbContext;

    public StudentsController(StudentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResultResponse<object>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResultResponse<object>>>> GetAll([FromQuery] CollectionQueryRequest request)
    {
        var query = _dbContext.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(s => s.FullName.Contains(search) || s.Email.Contains(search));
        }

        // Apply Sorting
        query = ApplySort(query, request.Sort);

        // Pagination
        var totalItems = await query.CountAsync();
        var page = request.Page;
        var pageSize = request.Size;
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var students = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StudentResponse
            {
                StudentId = s.StudentId,
                FullName = s.FullName,
                Email = s.Email,
                DateOfBirth = s.DateOfBirth
            })
            .ToListAsync();

        // Field Selection
        List<object> selectedItems;
        try
        {
            selectedItems = SelectFields(students, request.Fields);
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

        return Ok(ApiResponse<PagedResultResponse<object>>.Ok(result, "Students retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> GetById(int id)
    {
        var student = await _dbContext.Students.FindAsync(id);
        if (student is null)
        {
            return NotFound(ApiResponse<object>.Fail("Student not found."));
        }

        var response = new StudentResponse
        {
            StudentId = student.StudentId,
            FullName = student.FullName,
            Email = student.Email,
            DateOfBirth = student.DateOfBirth
        };

        return Ok(ApiResponse<StudentResponse>.Ok(response, "Student retrieved successfully."));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> Create([FromBody] CreateStudentRequest request)
    {
        if (await _dbContext.Students.AnyAsync(s => s.Email == request.Email.Trim()))
        {
            return BadRequest(ApiResponse<object>.Fail("Student email already exists."));
        }

        var student = new Student
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            DateOfBirth = request.DateOfBirth
        };

        await _dbContext.Students.AddAsync(student);
        await _dbContext.SaveChangesAsync();

        var response = new StudentResponse
        {
            StudentId = student.StudentId,
            FullName = student.FullName,
            Email = student.Email,
            DateOfBirth = student.DateOfBirth
        };

        return CreatedAtAction(nameof(GetById), new { id = student.StudentId }, ApiResponse<StudentResponse>.Ok(response, "Student created successfully."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Update(int id, [FromBody] UpdateStudentRequest request)
    {
        var student = await _dbContext.Students.FindAsync(id);
        if (student is null)
        {
            return NotFound(ApiResponse<object>.Fail("Student not found."));
        }

        if (await _dbContext.Students.AnyAsync(s => s.Email == request.Email.Trim() && s.StudentId != id))
        {
            return BadRequest(ApiResponse<object>.Fail("Student email already exists."));
        }

        student.FullName = request.FullName.Trim();
        student.Email = request.Email.Trim();
        student.DateOfBirth = request.DateOfBirth;

        _dbContext.Students.Update(student);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "Student updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var student = await _dbContext.Students.FindAsync(id);
        if (student is null)
        {
            return NotFound(ApiResponse<object>.Fail("Student not found."));
        }

        _dbContext.Students.Remove(student);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(null, "Student deleted successfully."));
    }

    private static IQueryable<Student> ApplySort(IQueryable<Student> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query.OrderBy(s => s.StudentId);
        }

        IOrderedQueryable<Student>? orderedQuery = null;

        foreach (var sortField in sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var descending = sortField.StartsWith('-');
            var field = descending ? sortField[1..] : sortField;

            if (field.Equals("studentid", StringComparison.OrdinalIgnoreCase))
            {
                orderedQuery = orderedQuery is null
                    ? descending ? query.OrderByDescending(s => s.StudentId) : query.OrderBy(s => s.StudentId)
                    : descending ? orderedQuery.ThenByDescending(s => s.StudentId) : orderedQuery.ThenBy(s => s.StudentId);
            }
            else if (field.Equals("fullname", StringComparison.OrdinalIgnoreCase))
            {
                orderedQuery = orderedQuery is null
                    ? descending ? query.OrderByDescending(s => s.FullName) : query.OrderBy(s => s.FullName)
                    : descending ? orderedQuery.ThenByDescending(s => s.FullName) : orderedQuery.ThenBy(s => s.FullName);
            }
            else if (field.Equals("email", StringComparison.OrdinalIgnoreCase))
            {
                orderedQuery = orderedQuery is null
                    ? descending ? query.OrderByDescending(s => s.Email) : query.OrderBy(s => s.Email)
                    : descending ? orderedQuery.ThenByDescending(s => s.Email) : orderedQuery.ThenBy(s => s.Email);
            }
            else if (field.Equals("dateofbirth", StringComparison.OrdinalIgnoreCase))
            {
                orderedQuery = orderedQuery is null
                    ? descending ? query.OrderByDescending(s => s.DateOfBirth) : query.OrderBy(s => s.DateOfBirth)
                    : descending ? orderedQuery.ThenByDescending(s => s.DateOfBirth) : orderedQuery.ThenBy(s => s.DateOfBirth);
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
