using Microsoft.Extensions.DependencyInjection;
using PRN232.LMS.Services.Implementations;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.Services;

public static class ServiceDependencyInjection
{
    public static IServiceCollection AddLmsServices(this IServiceCollection services)
    {
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<ISemesterService, SemesterService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();

        return services;
    }
}
