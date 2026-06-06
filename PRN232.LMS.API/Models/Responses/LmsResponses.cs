namespace PRN232.LMS.API.Models.Responses;

public class ApiResponse<T>
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public T? Data { get; set; }

    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T? data, string message = "Request processed successfully")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = null
        };
    }

    public static ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors?.ToList() ?? new List<string> { message }
        };
    }
}

public class PaginationMetadata
{
    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalItems { get; set; }

    public int TotalPages { get; set; }
}

public class PagedResultResponse<T>
{
    public List<T> Items { get; set; } = new();

    public PaginationMetadata Pagination { get; set; } = new();
}

public class AuthTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public int ExpiresIn { get; set; }
}

public class StudentResponse
{
    public int StudentId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public List<EnrollmentSummaryResponse>? Enrollments { get; set; }
}

public class StudentSummaryResponse
{
    public int StudentId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}

public class CourseResponse
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public int SemesterId { get; set; }

    public SemesterSummaryResponse? Semester { get; set; }

    public List<EnrollmentSummaryResponse>? Enrollments { get; set; }
}

public class CourseSummaryResponse
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public int SemesterId { get; set; }
}

public class SubjectResponse
{
    public int SubjectId { get; set; }

    public string SubjectCode { get; set; } = string.Empty;

    public string SubjectName { get; set; } = string.Empty;

    public int Credit { get; set; }
}

public class SemesterResponse
{
    public int SemesterId { get; set; }

    public string SemesterName { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public List<CourseSummaryResponse>? Courses { get; set; }
}

public class SemesterSummaryResponse
{
    public int SemesterId { get; set; }

    public string SemesterName { get; set; } = string.Empty;
}

public class EnrollmentResponse
{
    public int EnrollmentId { get; set; }

    public int StudentId { get; set; }

    public StudentSummaryResponse? Student { get; set; }

    public int CourseId { get; set; }

    public CourseSummaryResponse? Course { get; set; }

    public DateTime EnrollDate { get; set; }

    public string Status { get; set; } = string.Empty;
}

public class EnrollmentSummaryResponse
{
    public int EnrollmentId { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public DateTime EnrollDate { get; set; }

    public string Status { get; set; } = string.Empty;
}
