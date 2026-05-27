using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.Services.Interfaces;

public interface ISemesterService
{
    Task<PagedResultBusinessModel<SemesterBusinessModel>> GetAllAsync(CollectionQueryBusinessModel query);

    Task<SemesterBusinessModel?> GetByIdAsync(int id);

    Task<SemesterBusinessModel> CreateAsync(SemesterBusinessModel semester);

    Task<bool> UpdateAsync(int id, SemesterBusinessModel semester);

    Task<bool> DeleteAsync(int id);
}
