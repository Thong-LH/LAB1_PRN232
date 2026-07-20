using PRN232.LMS.Grpc;
using PRN232.LMS.CourseService.Models;

namespace PRN232.LMS.CourseService.Grpc;

public interface IStudentGrpcClient
{
    Task<StudentSummaryResponse?> GetStudentByIdAsync(int studentId);
}

public class StudentGrpcClient : IStudentGrpcClient
{
    private readonly StudentGrpc.StudentGrpcClient _grpcClient;
    private readonly ILogger<StudentGrpcClient> _logger;

    public StudentGrpcClient(StudentGrpc.StudentGrpcClient grpcClient, ILogger<StudentGrpcClient> logger)
    {
        _grpcClient = grpcClient;
        _logger = logger;
    }

    public async Task<StudentSummaryResponse?> GetStudentByIdAsync(int studentId)
    {
        try
        {
            _logger.LogInformation("Calling gRPC StudentService GetStudentById for ID: {StudentId}", studentId);
            
            var response = await _grpcClient.GetStudentByIdAsync(new GetStudentByIdRequest { StudentId = studentId });
            if (!response.Exists)
            {
                _logger.LogWarning("gRPC GetStudentById: Student {StudentId} does not exist.", studentId);
                return null;
            }

            return new StudentSummaryResponse
            {
                StudentId = response.StudentId,
                FullName = response.FullName,
                Email = response.Email
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while calling gRPC StudentService for student ID: {StudentId}", studentId);
            return null;
        }
    }
}

