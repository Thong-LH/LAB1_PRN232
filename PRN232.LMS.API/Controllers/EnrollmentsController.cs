using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Mappers;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResultResponse<object>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResultResponse<object>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PagedResultResponse<object>>>> GetEnrollments([FromQuery] CollectionQueryRequest query)
    {
        try
        {
            var enrollments = await _enrollmentService.GetAllAsync(query.ToBusinessModel());
            var response = enrollments.ToPagedResponse(enrollment => enrollment.ToResponse(), query.Fields);

            return Ok(ApiResponse<PagedResultResponse<object>>.Ok(response));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<PagedResultResponse<object>>.Fail(exception.Message));
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<EnrollmentResponse>>> GetEnrollment(int id)
    {
        var enrollment = await _enrollmentService.GetByIdAsync(id);

        if (enrollment is null)
        {
            return NotFound(ApiResponse<EnrollmentResponse>.Fail("Enrollment not found."));
        }

        return Ok(ApiResponse<EnrollmentResponse>.Ok(enrollment.ToResponse()));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<EnrollmentResponse>>> CreateEnrollment(CreateEnrollmentRequest request)
    {
        try
        {
            var enrollment = await _enrollmentService.CreateAsync(request.ToBusinessModel());
            var response = enrollment.ToResponse();

            return CreatedAtAction(
                nameof(GetEnrollment),
                new { id = response.EnrollmentId },
                ApiResponse<EnrollmentResponse>.Ok(response, "Enrollment created successfully."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<EnrollmentResponse>.Fail(exception.Message));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateEnrollment(int id, UpdateEnrollmentRequest request)
    {
        try
        {
            var updated = await _enrollmentService.UpdateAsync(id, request.ToBusinessModel());

            if (!updated)
            {
                return NotFound(ApiResponse<object>.Fail("Enrollment not found."));
            }

            return Ok(ApiResponse<object>.Ok(null, "Enrollment updated successfully."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(exception.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteEnrollment(int id)
    {
        var deleted = await _enrollmentService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(ApiResponse<object>.Fail("Enrollment not found."));
        }

        return Ok(ApiResponse<object>.Ok(null, "Enrollment deleted successfully."));
    }
}
