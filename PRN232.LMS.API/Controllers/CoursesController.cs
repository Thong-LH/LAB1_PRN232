using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Mappers;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;

    public CoursesController(ICourseService courseService, IEnrollmentService enrollmentService)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResultResponse<object>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResultResponse<object>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PagedResultResponse<object>>>> GetCourses([FromQuery] CollectionQueryRequest query)
    {
        try
        {
            var courses = await _courseService.GetAllAsync(query.ToBusinessModel());
            var response = courses.ToPagedResponse(course => course.ToResponse(), query.Fields);

            return Ok(ApiResponse<PagedResultResponse<object>>.Ok(response));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<PagedResultResponse<object>>.Fail(exception.Message));
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CourseResponse>>> GetCourse(int id)
    {
        var course = await _courseService.GetByIdAsync(id);

        if (course is null)
        {
            return NotFound(ApiResponse<CourseResponse>.Fail("Course not found."));
        }

        return Ok(ApiResponse<CourseResponse>.Ok(course.ToResponse()));
    }

    [HttpGet("{id:int}/enrollments")]
    [ProducesResponseType(typeof(ApiResponse<PagedResultResponse<object>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResultResponse<object>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PagedResultResponse<object>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedResultResponse<object>>>> GetCourseEnrollments(int id, [FromQuery] CollectionQueryRequest query)
    {
        try
        {
            var enrollments = await _enrollmentService.GetByCourseIdAsync(id, query.ToBusinessModel());

            if (enrollments is null)
            {
                return NotFound(ApiResponse<PagedResultResponse<object>>.Fail("Course not found."));
            }

            var response = enrollments.ToPagedResponse(enrollment => enrollment.ToResponse(), query.Fields);

            return Ok(ApiResponse<PagedResultResponse<object>>.Ok(response));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<PagedResultResponse<object>>.Fail(exception.Message));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CourseResponse>>> CreateCourse(CreateCourseRequest request)
    {
        try
        {
            var course = await _courseService.CreateAsync(request.ToBusinessModel());
            var response = course.ToResponse();

            return CreatedAtAction(
                nameof(GetCourse),
                new { id = response.CourseId },
                ApiResponse<CourseResponse>.Ok(response, "Course created successfully."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<CourseResponse>.Fail(exception.Message));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateCourse(int id, UpdateCourseRequest request)
    {
        try
        {
            var updated = await _courseService.UpdateAsync(id, request.ToBusinessModel());

            if (!updated)
            {
                return NotFound(ApiResponse<object>.Fail("Course not found."));
            }

            return Ok(ApiResponse<object>.Ok(null, "Course updated successfully."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(exception.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCourse(int id)
    {
        var deleted = await _courseService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(ApiResponse<object>.Fail("Course not found."));
        }

        return Ok(ApiResponse<object>.Ok(null, "Course deleted successfully."));
    }
}
