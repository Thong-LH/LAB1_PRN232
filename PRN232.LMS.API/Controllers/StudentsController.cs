using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Mappers;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResultResponse<object>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResultResponse<object>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PagedResultResponse<object>>>> GetStudents([FromQuery] CollectionQueryRequest query)
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
    public async Task<ActionResult<ApiResponse<StudentResponse>>> GetStudent(int id)
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
    public async Task<ActionResult<ApiResponse<StudentResponse>>> CreateStudent(CreateStudentRequest request)
    {
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
    public async Task<ActionResult<ApiResponse<object>>> UpdateStudent(int id, UpdateStudentRequest request)
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
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteStudent(int id)
    {
        var deleted = await _studentService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(ApiResponse<object>.Fail("Student not found."));
        }

        return Ok(ApiResponse<object>.Ok(null, "Student deleted successfully."));
    }
}
