using FluentValidation;
using PortfolioCMS.API.DTOs;
using PortfolioCMS.API.Utilities;

namespace PortfolioCMS.API.Validators;

/// <summary>
/// Validator for CreateArticleDto with security-focused validation rules
/// </summary>
public class CreateArticleDtoValidator : AbstractValidator<CreateArticleDto>
{
    public CreateArticleDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters")
            .Must(BeValidTitle).WithMessage("Title contains invalid characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .MaximumLength(50000).WithMessage("Content must not exceed 50000 characters")
            .Must(BeSafeHtml).WithMessage("Content contains potentially dangerous HTML");

        RuleFor(x => x.Summary)
            .MaximumLength(500).WithMessage("Summary must not exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Summary));

        RuleFor(x => x.FeaturedImageUrl)
            .Must(BeValidUrl).WithMessage("Featured image URL is invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.FeaturedImageUrl));

        RuleFor(x => x.MetaDescription)
            .MaximumLength(160).WithMessage("Meta description must not exceed 160 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.MetaDescription));

        RuleFor(x => x.MetaKeywords)
            .MaximumLength(255).WithMessage("Meta keywords must not exceed 255 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.MetaKeywords));

        // TagIds validation can be added when the property exists in CreateArticleDto
    }

    private bool BeValidTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return false;

        // Check for dangerous patterns
        var sanitized = InputSanitizer.StripHtml(title);
        return sanitized.Length > 0 && sanitized.Length == title.Trim().Length;
    }

    private bool BeSafeHtml(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;

        // Check for script tags and dangerous patterns
        var dangerous = new[] { "<script", "javascript:", "onerror=", "onload=" };
        return !dangerous.Any(pattern => 
            content.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    private bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        var sanitized = InputSanitizer.SanitizeUrl(url);
        return sanitized != null;
    }
}
