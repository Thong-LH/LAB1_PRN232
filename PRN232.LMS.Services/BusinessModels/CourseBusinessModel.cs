namespace PRN232.LMS.Services.BusinessModels;

public class CourseBusinessModel
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public int SemesterId { get; set; }

    public SemesterBusinessModel? Semester { get; set; }

    public IReadOnlyList<EnrollmentBusinessModel>? Enrollments { get; set; }
}
