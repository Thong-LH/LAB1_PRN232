using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Mappers;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/subjects")]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResultResponse<object>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResultResponse<object>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PagedResultResponse<object>>>> GetSubjects([FromQuery] CollectionQueryRequest query)
    {
        try
        {
            var subjects = await _subjectService.GetAllAsync(query.ToBusinessModel());
            var response = subjects.ToPagedResponse(subject => subject.ToResponse(), query.Fields);

            return Ok(ApiResponse<PagedResultResponse<object>>.Ok(response));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<PagedResultResponse<object>>.Fail(exception.Message));
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SubjectResponse>>> GetSubject(int id)
    {
        var subject = await _subjectService.GetByIdAsync(id);

        if (subject is null)
        {
            return NotFound(ApiResponse<SubjectResponse>.Fail("Subject not found."));
        }

        return Ok(ApiResponse<SubjectResponse>.Ok(subject.ToResponse()));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SubjectResponse>>> CreateSubject(CreateSubjectRequest request)
    {
        try
        {
            var subject = await _subjectService.CreateAsync(request.ToBusinessModel());
            var response = subject.ToResponse();

            return CreatedAtAction(
                nameof(GetSubject),
                new { id = response.SubjectId },
                ApiResponse<SubjectResponse>.Ok(response, "Subject created successfully."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<SubjectResponse>.Fail(exception.Message));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateSubject(int id, UpdateSubjectRequest request)
    {
        try
        {
            var updated = await _subjectService.UpdateAsync(id, request.ToBusinessModel());

            if (!updated)
            {
                return NotFound(ApiResponse<object>.Fail("Subject not found."));
            }

            return Ok(ApiResponse<object>.Ok(null, "Subject updated successfully."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(exception.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSubject(int id)
    {
        var deleted = await _subjectService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(ApiResponse<object>.Fail("Subject not found."));
        }

        return Ok(ApiResponse<object>.Ok(null, "Subject deleted successfully."));
    }
}
