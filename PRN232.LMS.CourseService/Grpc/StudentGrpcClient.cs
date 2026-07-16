using Grpc.Net.Client;
using PRN232.LMS.Grpc;
using PRN232.LMS.CourseService.Models;

namespace PRN232.LMS.CourseService.Grpc;

public interface IStudentGrpcClient
{
    Task<StudentSummaryResponse?> GetStudentByIdAsync(int studentId);
}

public class StudentGrpcClient : IStudentGrpcClient
{
    private readonly string _studentServiceUrl;
    private readonly ILogger<StudentGrpcClient> _logger;

    public StudentGrpcClient(IConfiguration configuration, ILogger<StudentGrpcClient> logger)
    {
        _studentServiceUrl = configuration["GrpcServices:StudentServiceUrl"] ?? "http://localhost:5002";
        _logger = logger;
    }

    public async Task<StudentSummaryResponse?> GetStudentByIdAsync(int studentId)
    {
        try
        {
            _logger.LogInformation("Calling gRPC StudentService GetStudentById for ID: {StudentId} at URL {Url}", studentId, _studentServiceUrl);
            using var channel = GrpcChannel.ForAddress(_studentServiceUrl);
            var client = new StudentGrpc.StudentGrpcClient(channel);
            
            var response = await client.GetStudentByIdAsync(new GetStudentRequest { Id = studentId });
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
