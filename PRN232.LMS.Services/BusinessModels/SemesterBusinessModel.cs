namespace PRN232.LMS.Services.BusinessModels;

public class SemesterBusinessModel
{
    public int SemesterId { get; set; }

    public string SemesterName { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public IReadOnlyList<CourseBusinessModel>? Courses { get; set; }
}
