using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Mappers;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/semesters")]
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _semesterService;

    public SemestersController(ISemesterService semesterService)
    {
        _semesterService = semesterService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResultResponse<object>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResultResponse<object>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PagedResultResponse<object>>>> GetSemesters([FromQuery] CollectionQueryRequest query)
    {
        try
        {
            var semesters = await _semesterService.GetAllAsync(query.ToBusinessModel());
            var response = semesters.ToPagedResponse(semester => semester.ToResponse(), query.Fields);

            return Ok(ApiResponse<PagedResultResponse<object>>.Ok(response));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<PagedResultResponse<object>>.Fail(exception.Message));
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SemesterResponse>>> GetSemester(int id)
    {
        var semester = await _semesterService.GetByIdAsync(id);

        if (semester is null)
        {
            return NotFound(ApiResponse<SemesterResponse>.Fail("Semester not found."));
        }

        return Ok(ApiResponse<SemesterResponse>.Ok(semester.ToResponse()));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SemesterResponse>>> CreateSemester(CreateSemesterRequest request)
    {
        try
        {
            var semester = await _semesterService.CreateAsync(request.ToBusinessModel());
            var response = semester.ToResponse();

            return CreatedAtAction(
                nameof(GetSemester),
                new { id = response.SemesterId },
                ApiResponse<SemesterResponse>.Ok(response, "Semester created successfully."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<SemesterResponse>.Fail(exception.Message));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateSemester(int id, UpdateSemesterRequest request)
    {
        try
        {
            var updated = await _semesterService.UpdateAsync(id, request.ToBusinessModel());

            if (!updated)
            {
                return NotFound(ApiResponse<object>.Fail("Semester not found."));
            }

            return Ok(ApiResponse<object>.Ok(null, "Semester updated successfully."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(exception.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSemester(int id)
    {
        var deleted = await _semesterService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(ApiResponse<object>.Fail("Semester not found."));
        }

        return Ok(ApiResponse<object>.Ok(null, "Semester deleted successfully."));
    }
}
