using DotCruz.Notifications.Application.UseCases.Tenants.ConfigureTenantBranding;
using System.Threading.Tasks;
using Xunit;

namespace Validators.Test.Tenants;

public class ConfigureTenantBrandingCommandValidatorTests
{
    [Fact]
    public async Task Success()
    {
        var validator = new ConfigureTenantBrandingCommandValidator();
        var command = new ConfigureTenantBrandingCommand(
            TenantName: "My Shop",
            TenantLogoUrl: "https://myshop.com/logo.png",
            TenantWebsite: "https://myshop.com",
            TenantAddress: "123 Main St, Springfield",
            UnsubscribeUrl: "https://myshop.com/unsubscribe",
            HeaderBackgroundColor: "#ffffff"
        );

        var result = await validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Error_Fields_Empty()
    {
        var validator = new ConfigureTenantBrandingCommandValidator();
        var command = new ConfigureTenantBrandingCommand(
            TenantName: "",
            TenantLogoUrl: "",
            TenantWebsite: "",
            TenantAddress: "",
            UnsubscribeUrl: ""
        );

        var result = await validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ConfigureTenantBrandingCommand.TenantName));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ConfigureTenantBrandingCommand.TenantLogoUrl));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ConfigureTenantBrandingCommand.TenantWebsite));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ConfigureTenantBrandingCommand.TenantAddress));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ConfigureTenantBrandingCommand.UnsubscribeUrl));
    }

    [Theory]
    [InlineData("invalid-url")]
    [InlineData("/relative/path")]
    [InlineData("ftp://myshop.com")]
    public async Task Error_Invalid_Urls(string invalidUrl)
    {
        var validator = new ConfigureTenantBrandingCommandValidator();
        var command = new ConfigureTenantBrandingCommand(
            TenantName: "My Shop",
            TenantLogoUrl: invalidUrl,
            TenantWebsite: invalidUrl,
            TenantAddress: "123 Main St",
            UnsubscribeUrl: invalidUrl
        );

        var result = await validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ConfigureTenantBrandingCommand.TenantLogoUrl));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ConfigureTenantBrandingCommand.TenantWebsite));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ConfigureTenantBrandingCommand.UnsubscribeUrl));
    }

    [Theory]
    [InlineData("white")]
    [InlineData("#")]
    [InlineData("#ff")]
    [InlineData("#ffff")]
    [InlineData("#1234567")]
    public async Task Error_Invalid_HexColor(string invalidColor)
    {
        var validator = new ConfigureTenantBrandingCommandValidator();
        var command = new ConfigureTenantBrandingCommand(
            TenantName: "My Shop",
            TenantLogoUrl: "https://myshop.com/logo.png",
            TenantWebsite: "https://myshop.com",
            TenantAddress: "123 Main St",
            UnsubscribeUrl: "https://myshop.com/unsubscribe",
            HeaderBackgroundColor: invalidColor
        );

        var result = await validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ConfigureTenantBrandingCommand.HeaderBackgroundColor));
    }
}
