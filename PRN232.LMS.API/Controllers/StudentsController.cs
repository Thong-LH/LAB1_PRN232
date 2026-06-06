using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Mappers;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/students")]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly IValidator<CreateStudentRequest> _createStudentValidator;

    public StudentsController(IStudentService studentService, IValidator<CreateStudentRequest> createStudentValidator)
    {
        _studentService = studentService;
        _createStudentValidator = createStudentValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResultResponse<object>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResultResponse<object>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PagedResultResponse<object>>>> GetStudents(
        [FromQuery] CollectionQueryRequest query,
        [FromHeader(Name = "X-Request-Id")] string? requestId)
    {
        try
        {
            var students = await _studentService.GetAllAsync(query.ToBusinessModel());
            var response = students.ToPagedResponse(student => student.ToResponse(), query.Fields);

            return Ok(ApiResponse<PagedResultResponse<object>>.Ok(response));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<PagedResultResponse<object>>.Fail(exception.Message));
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> GetStudent(
        [FromRoute] int id,
        [FromHeader(Name = "X-Request-Id")] string? requestId)
    {
        var student = await _studentService.GetByIdAsync(id);

        if (student is null)
        {
            return NotFound(ApiResponse<StudentResponse>.Fail("Student not found."));
        }

        return Ok(ApiResponse<StudentResponse>.Ok(student.ToResponse()));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> CreateStudent([FromBody] CreateStudentRequest request)
    {
        var validationResult = await _createStudentValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return BadRequest(ApiResponse<StudentResponse>.Fail(
                "Validation failed.",
                validationResult.Errors.Select(error => error.ErrorMessage).ToList()));
        }

        try
        {
            var student = await _studentService.CreateAsync(request.ToBusinessModel());
            var response = student.ToResponse();

            return CreatedAtAction(
                nameof(GetStudent),
                new { id = response.StudentId },
                ApiResponse<StudentResponse>.Ok(response, "Student created successfully."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<StudentResponse>.Fail(exception.Message));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateStudent([FromRoute] int id, [FromBody] UpdateStudentRequest request)
    {
        try
        {
            var updated = await _studentService.UpdateAsync(id, request.ToBusinessModel());

            if (!updated)
            {
                return NotFound(ApiResponse<object>.Fail("Student not found."));
            }

            return Ok(ApiResponse<object>.Ok(null, "Student updated successfully."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(exception.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteStudent([FromRoute] int id)
    {
        var deleted = await _studentService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(ApiResponse<object>.Fail("Student not found."));
        }

        return Ok(ApiResponse<object>.Ok(null, "Student deleted successfully."));
    }
}
