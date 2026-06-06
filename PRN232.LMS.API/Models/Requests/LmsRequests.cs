using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public class CollectionQueryRequest
{
    public string? Search { get; set; }

    public string? Sort { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int Size { get; set; } = 10;

    public string? Fields { get; set; }

    public string? Expand { get; set; }
}

public class LoginRequest
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class CreateStudentRequest
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [RegularExpression("^(SE|CE|HE)\\d{5}$", ErrorMessage = "Student code must follow FPTU style, for example SE19886 or CE18793.")]
    public string? StudentCode { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }
}

public class UpdateStudentRequest : CreateStudentRequest
{
}

public class CreateCourseRequest
{
    [Required]
    [StringLength(100)]
    public string CourseName { get; set; } = string.Empty;

    [Required]
    public int SemesterId { get; set; }
}

public class UpdateCourseRequest : CreateCourseRequest
{
}

public class CreateSubjectRequest
{
    [Required]
    [StringLength(20)]
    public string SubjectCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string SubjectName { get; set; } = string.Empty;

    [Range(1, 10)]
    public int Credit { get; set; }
}

public class UpdateSubjectRequest : CreateSubjectRequest
{
}

public class CreateSemesterRequest
{
    [Required]
    [StringLength(100)]
    public string SemesterName { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}

public class UpdateSemesterRequest : CreateSemesterRequest
{
}

public class CreateEnrollmentRequest
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int CourseId { get; set; }

    [Required]
    public DateTime EnrollDate { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = string.Empty;
}

public class UpdateEnrollmentRequest : CreateEnrollmentRequest
{
}
