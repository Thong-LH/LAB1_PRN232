using PRN232.LMS.API.Helpers;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.API.Mappers;

public static class ApiModelMapper
{
    public static CollectionQueryBusinessModel ToBusinessModel(this CollectionQueryRequest request)
    {
        return new CollectionQueryBusinessModel
        {
            Search = request.Search,
            Sort = request.Sort,
            Page = request.Page,
            PageSize = request.Size,
            Expand = request.Expand
        };
    }

    public static PagedResultResponse<object> ToPagedResponse<TBusinessModel, TResponse>(
        this PagedResultBusinessModel<TBusinessModel> result,
        Func<TBusinessModel, TResponse> responseMapper,
        string? fields)
        where TResponse : class
    {
        var responses = result.Items.Select(responseMapper).ToList();

        return new PagedResultResponse<object>
        {
            Items = FieldSelectionHelper.SelectFields(responses, fields),
            Pagination = new PaginationMetadata
            {
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            }
        };
    }

    public static StudentBusinessModel ToBusinessModel(this CreateStudentRequest request)
    {
        return new StudentBusinessModel
        {
            FullName = request.FullName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth
        };
    }

    public static CourseBusinessModel ToBusinessModel(this CreateCourseRequest request)
    {
        return new CourseBusinessModel
        {
            CourseName = request.CourseName,
            SemesterId = request.SemesterId
        };
    }

    public static SubjectBusinessModel ToBusinessModel(this CreateSubjectRequest request)
    {
        return new SubjectBusinessModel
        {
            SubjectCode = request.SubjectCode,
            SubjectName = request.SubjectName,
            Credit = request.Credit
        };
    }

    public static SemesterBusinessModel ToBusinessModel(this CreateSemesterRequest request)
    {
        return new SemesterBusinessModel
        {
            SemesterName = request.SemesterName,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
    }

    public static EnrollmentBusinessModel ToBusinessModel(this CreateEnrollmentRequest request)
    {
        return new EnrollmentBusinessModel
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            EnrollDate = request.EnrollDate,
            Status = request.Status
        };
    }

    public static StudentResponse ToResponse(this StudentBusinessModel student)
    {
        return new StudentResponse
        {
            StudentId = student.StudentId,
            FullName = student.FullName,
            Email = student.Email,
            DateOfBirth = student.DateOfBirth,
            Enrollments = student.Enrollments?.Select(enrollment => enrollment.ToSummaryResponse()).ToList()
        };
    }

    public static StudentSummaryResponse ToSummaryResponse(this StudentBusinessModel student)
    {
        return new StudentSummaryResponse
        {
            StudentId = student.StudentId,
            FullName = student.FullName,
            Email = student.Email
        };
    }

    public static CourseResponse ToResponse(this CourseBusinessModel course)
    {
        return new CourseResponse
        {
            CourseId = course.CourseId,
            CourseName = course.CourseName,
            SemesterId = course.SemesterId,
            Semester = course.Semester?.ToSummaryResponse(),
            Enrollments = course.Enrollments?.Select(enrollment => enrollment.ToSummaryResponse()).ToList()
        };
    }

    public static CourseSummaryResponse ToSummaryResponse(this CourseBusinessModel course)
    {
        return new CourseSummaryResponse
        {
            CourseId = course.CourseId,
            CourseName = course.CourseName,
            SemesterId = course.SemesterId
        };
    }

    public static SubjectResponse ToResponse(this SubjectBusinessModel subject)
    {
        return new SubjectResponse
        {
            SubjectId = subject.SubjectId,
            SubjectCode = subject.SubjectCode,
            SubjectName = subject.SubjectName,
            Credit = subject.Credit
        };
    }

    public static SemesterResponse ToResponse(this SemesterBusinessModel semester)
    {
        return new SemesterResponse
        {
            SemesterId = semester.SemesterId,
            SemesterName = semester.SemesterName,
            StartDate = semester.StartDate,
            EndDate = semester.EndDate,
            Courses = semester.Courses?.Select(course => course.ToSummaryResponse()).ToList()
        };
    }

    public static SemesterSummaryResponse ToSummaryResponse(this SemesterBusinessModel semester)
    {
        return new SemesterSummaryResponse
        {
            SemesterId = semester.SemesterId,
            SemesterName = semester.SemesterName
        };
    }

    public static EnrollmentResponse ToResponse(this EnrollmentBusinessModel enrollment)
    {
        return new EnrollmentResponse
        {
            EnrollmentId = enrollment.EnrollmentId,
            StudentId = enrollment.StudentId,
            Student = enrollment.Student?.ToSummaryResponse(),
            CourseId = enrollment.CourseId,
            Course = enrollment.Course?.ToSummaryResponse(),
            EnrollDate = enrollment.EnrollDate,
            Status = enrollment.Status
        };
    }

    public static EnrollmentSummaryResponse ToSummaryResponse(this EnrollmentBusinessModel enrollment)
    {
        return new EnrollmentSummaryResponse
        {
            EnrollmentId = enrollment.EnrollmentId,
            StudentId = enrollment.StudentId,
            CourseId = enrollment.CourseId,
            EnrollDate = enrollment.EnrollDate,
            Status = enrollment.Status
        };
    }
}
