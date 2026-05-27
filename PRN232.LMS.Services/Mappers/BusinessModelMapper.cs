using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.Services.Mappers;

public static class BusinessModelMapper
{
    public static StudentBusinessModel ToBusinessModel(this Student student, bool includeEnrollments = false)
    {
        return new StudentBusinessModel
        {
            StudentId = student.StudentId,
            FullName = student.FullName,
            Email = student.Email,
            DateOfBirth = student.DateOfBirth,
            Enrollments = includeEnrollments
                ? student.Enrollments.Select(enrollment => enrollment.ToBusinessModel()).ToList()
                : null
        };
    }

    public static CourseBusinessModel ToBusinessModel(this Course course, bool includeSemester = false, bool includeEnrollments = false)
    {
        return new CourseBusinessModel
        {
            CourseId = course.CourseId,
            CourseName = course.CourseName,
            SemesterId = course.SemesterId,
            Semester = includeSemester && course.Semester is not null
                ? course.Semester.ToBusinessModel()
                : null,
            Enrollments = includeEnrollments
                ? course.Enrollments.Select(enrollment => enrollment.ToBusinessModel()).ToList()
                : null
        };
    }

    public static SubjectBusinessModel ToBusinessModel(this Subject subject)
    {
        return new SubjectBusinessModel
        {
            SubjectId = subject.SubjectId,
            SubjectCode = subject.SubjectCode,
            SubjectName = subject.SubjectName,
            Credit = subject.Credit
        };
    }

    public static SemesterBusinessModel ToBusinessModel(this Semester semester, bool includeCourses = false)
    {
        return new SemesterBusinessModel
        {
            SemesterId = semester.SemesterId,
            SemesterName = semester.SemesterName,
            StartDate = semester.StartDate,
            EndDate = semester.EndDate,
            Courses = includeCourses
                ? semester.Courses.Select(course => course.ToBusinessModel()).ToList()
                : null
        };
    }

    public static EnrollmentBusinessModel ToBusinessModel(this Enrollment enrollment, bool includeStudent = false, bool includeCourse = false)
    {
        return new EnrollmentBusinessModel
        {
            EnrollmentId = enrollment.EnrollmentId,
            StudentId = enrollment.StudentId,
            Student = includeStudent && enrollment.Student is not null
                ? enrollment.Student.ToBusinessModel()
                : null,
            CourseId = enrollment.CourseId,
            Course = includeCourse && enrollment.Course is not null
                ? enrollment.Course.ToBusinessModel(includeSemester: true)
                : null,
            EnrollDate = enrollment.EnrollDate,
            Status = enrollment.Status
        };
    }
}
