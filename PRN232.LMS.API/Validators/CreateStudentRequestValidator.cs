using FluentValidation;
using PRN232.LMS.API.Models.Requests;

namespace PRN232.LMS.API.Validators;

public class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator()
    {
        RuleFor(request => request.FullName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(100);

        RuleFor(request => request.StudentCode)
            .Must(BeValidFptuStudentCode)
            .When(request => !string.IsNullOrWhiteSpace(request.StudentCode))
            .WithMessage("Student code must follow FPTU style, for example SE19886 or CE18793.");
    }

    private static bool BeValidFptuStudentCode(string? studentCode)
    {
        if (string.IsNullOrWhiteSpace(studentCode))
        {
            return true;
        }

        return System.Text.RegularExpressions.Regex.IsMatch(studentCode, "^(SE|CE|HE)\\d{5}$");
    }
}
