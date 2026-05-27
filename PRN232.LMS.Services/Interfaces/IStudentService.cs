using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.Services.Interfaces;

public interface IStudentService
{
    Task<PagedResultBusinessModel<StudentBusinessModel>> GetAllAsync(CollectionQueryBusinessModel query);

    Task<StudentBusinessModel?> GetByIdAsync(int id);

    Task<StudentBusinessModel> CreateAsync(StudentBusinessModel student);

    Task<bool> UpdateAsync(int id, StudentBusinessModel student);

    Task<bool> DeleteAsync(int id);
}
