using Grpc.Core;
using PRN232.LMS.Grpc;
using PRN232.LMS.StudentService.Data;

namespace PRN232.LMS.StudentService.Grpc;

public class StudentGrpcService : StudentGrpc.StudentGrpcBase
{
    private readonly StudentDbContext _dbContext;
    private readonly ILogger<StudentGrpcService> _logger;

    public StudentGrpcService(StudentDbContext dbContext, ILogger<StudentGrpcService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public override async Task<StudentResponse> GetStudentById(GetStudentRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC GetStudentById called for ID: {StudentId}", request.Id);
        
        var student = await _dbContext.Students.FindAsync(request.Id);
        if (student is null)
        {
            _logger.LogWarning("gRPC GetStudentById: Student with ID {StudentId} not found.", request.Id);
            return new StudentResponse
            {
                Exists = false
            };
        }

        return new StudentResponse
        {
            StudentId = student.StudentId,
            FullName = student.FullName,
            Email = student.Email,
            Exists = true
        };
    }
}
