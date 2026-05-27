using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.Services.Interfaces;

public interface ICourseService
{
    Task<PagedResultBusinessModel<CourseBusinessModel>> GetAllAsync(CollectionQueryBusinessModel query);

    Task<CourseBusinessModel?> GetByIdAsync(int id);

    Task<CourseBusinessModel> CreateAsync(CourseBusinessModel course);

    Task<bool> UpdateAsync(int id, CourseBusinessModel course);

    Task<bool> DeleteAsync(int id);
}
