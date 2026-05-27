using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.Services.Interfaces;

public interface IEnrollmentService
{
    Task<PagedResultBusinessModel<EnrollmentBusinessModel>> GetAllAsync(CollectionQueryBusinessModel query);

    Task<PagedResultBusinessModel<EnrollmentBusinessModel>?> GetByCourseIdAsync(int courseId, CollectionQueryBusinessModel query);

    Task<EnrollmentBusinessModel?> GetByIdAsync(int id);

    Task<EnrollmentBusinessModel> CreateAsync(EnrollmentBusinessModel enrollment);

    Task<bool> UpdateAsync(int id, EnrollmentBusinessModel enrollment);

    Task<bool> DeleteAsync(int id);
}
