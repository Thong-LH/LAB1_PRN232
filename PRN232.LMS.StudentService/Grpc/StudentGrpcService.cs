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

    public override async Task<GetStudentByIdResponse> GetStudentById(GetStudentByIdRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC GetStudentById called for ID: {StudentId}", request.StudentId);
        
        var student = await _dbContext.Students.FindAsync(request.StudentId);
        if (student is null)
        {
            _logger.LogWarning("gRPC GetStudentById: Student with ID {StudentId} not found.", request.StudentId);
            return new GetStudentByIdResponse
            {
                Exists = false
            };
        }

        return new GetStudentByIdResponse
        {
            StudentId = student.StudentId,
            FullName = student.FullName,
            Email = student.Email,
            Exists = true
        };
    }
}
