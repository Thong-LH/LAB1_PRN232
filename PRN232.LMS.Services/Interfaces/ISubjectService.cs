using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.Services.Interfaces;

public interface ISubjectService
{
    Task<PagedResultBusinessModel<SubjectBusinessModel>> GetAllAsync(CollectionQueryBusinessModel query);

    Task<SubjectBusinessModel?> GetByIdAsync(int id);

    Task<SubjectBusinessModel> CreateAsync(SubjectBusinessModel subject);

    Task<bool> UpdateAsync(int id, SubjectBusinessModel subject);

    Task<bool> DeleteAsync(int id);
}
