using DotCruz.Notifications.Domain.Exceptions.Resources;
using FluentValidation;
using System;
using System.Text.RegularExpressions;

namespace DotCruz.Notifications.Application.UseCases.Tenants.ConfigureTenantBranding;

public class ConfigureTenantBrandingCommandValidator : AbstractValidator<ConfigureTenantBrandingCommand>
{
    private static readonly Regex HexColorRegex = new(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", RegexOptions.Compiled);

    public ConfigureTenantBrandingCommandValidator()
    {
        RuleFor(x => x.TenantName)
            .NotEmpty()
            .WithMessage(ResourceMessagesException.TENANT_NAME_EMPTY);

        RuleFor(x => x.TenantLogoUrl)
            .NotEmpty()
            .WithMessage(ResourceMessagesException.TENANT_LOGO_URL_EMPTY)
            .Must(BeValidUrl)
            .WithMessage(ResourceMessagesException.TENANT_LOGO_URL_INVALID);

        RuleFor(x => x.TenantWebsite)
            .NotEmpty()
            .WithMessage(ResourceMessagesException.TENANT_WEBSITE_URL_EMPTY)
            .Must(BeValidUrl)
            .WithMessage(ResourceMessagesException.TENANT_WEBSITE_URL_INVALID);

        RuleFor(x => x.TenantAddress)
            .NotEmpty()
            .WithMessage(ResourceMessagesException.TENANT_ADDRESS_EMPTY);

        RuleFor(x => x.UnsubscribeUrl)
            .NotEmpty()
            .WithMessage(ResourceMessagesException.UNSUBSCRIBE_URL_EMPTY)
            .Must(BeValidUrl)
            .WithMessage(ResourceMessagesException.UNSUBSCRIBE_URL_INVALID);

        RuleFor(x => x.HeaderBackgroundColor)
            .Must(BeValidHexColor)
            .WithMessage(ResourceMessagesException.HEADER_BACKGROUND_COLOR_INVALID)
            .When(x => !string.IsNullOrEmpty(x.HeaderBackgroundColor));
    }

    private static bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result) 
            && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    private static bool BeValidHexColor(string hexColor)
    {
        return HexColorRegex.IsMatch(hexColor);
    }
}
