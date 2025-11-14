using FluentValidation;
using PortfolioCMS.API.DTOs;
using PortfolioCMS.API.Utilities;

namespace PortfolioCMS.API.Validators;

/// <summary>
/// Validator for CreateCommentDto with security-focused validation rules
/// </summary>
public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentDtoValidator()
    {
        RuleFor(x => x.AuthorName)
            .NotEmpty().WithMessage("Author name is required")
            .MaximumLength(100).WithMessage("Author name must not exceed 100 characters")
            .Must(BeValidName).WithMessage("Author name contains invalid characters");

        RuleFor(x => x.AuthorEmail)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters")
            .Must(BeValidEmail).WithMessage("Email contains invalid characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required")
            .MaximumLength(2000).WithMessage("Comment must not exceed 2000 characters")
            .Must(BeSafeContent).WithMessage("Comment contains potentially dangerous content");

        RuleFor(x => x.ArticleId)
            .GreaterThan(0).WithMessage("Valid article ID is required");
    }

    private bool BeValidName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        // Strip HTML and check if anything remains
        var sanitized = InputSanitizer.StripHtml(name);
        return sanitized.Length > 0;
    }

    private bool BeValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var sanitized = InputSanitizer.SanitizeEmail(email);
        return sanitized != null;
    }

    private bool BeSafeContent(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;

        // Check for dangerous patterns
        var dangerous = new[] { "<script", "javascript:", "<iframe", "onerror=", "onload=" };
        return !dangerous.Any(pattern => 
            content.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }
}
