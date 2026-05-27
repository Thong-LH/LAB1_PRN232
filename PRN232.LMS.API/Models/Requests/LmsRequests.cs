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

public class CreateStudentRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }
}

public class UpdateStudentRequest : CreateStudentRequest
{
}

public class CreateCourseRequest
{
    [Required]
    [MaxLength(100)]
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
    [MaxLength(20)]
    public string SubjectCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
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
    [MaxLength(100)]
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
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;
}

public class UpdateEnrollmentRequest : CreateEnrollmentRequest
{
}
