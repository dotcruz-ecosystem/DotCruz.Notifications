using DotCruz.Notifications.Application.UseCases.Tenants.ConfigureTenantBranding;
using DotCruz.Notifications.Domain.Entities.Tenants;
using DotCruz.Notifications.Domain.Exceptions.BaseExceptions;
using DotCruz.Notifications.Domain.Interfaces;
using DotCruz.Notifications.Domain.Interfaces.Repositories;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace UseCases.Test.Tenants;

public class ConfigureTenantBrandingCommandHandlerTests
{
    [Fact]
    public async Task Success_CreateNewSettings()
    {
        var tenantId = Guid.NewGuid();
        var command = new ConfigureTenantBrandingCommand(
            TenantName: "Shop A",
            TenantLogoUrl: "https://shopa.com/logo.png",
            TenantWebsite: "https://shopa.com",
            TenantAddress: "Street 1",
            UnsubscribeUrl: "https://shopa.com/optout",
            HeaderBackgroundColor: "#fafafa"
        );

        var tenantProviderMock = new Mock<ITenantProvider>();
        tenantProviderMock.Setup(x => x.TenantId).Returns(tenantId);

        var repositoryMock = new Mock<ITenantSettingsRepository>();
        repositoryMock.Setup(x => x.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantSettings?)null);

        var handler = new ConfigureTenantBrandingCommandHandler(repositoryMock.Object, tenantProviderMock.Object);

        await handler.Handle(command, TestContext.Current.CancellationToken);

        repositoryMock.Verify(x => x.AddAsync(
            It.Is<TenantSettings>(s => 
                s.TenantId == tenantId && 
                s.TenantName == "Shop A" && 
                s.TenantLogoUrl == "https://shopa.com/logo.png" && 
                s.HeaderBackgroundColor == "#fafafa" && 
                s.TenantAddress == "Street 1" && 
                s.UnsubscribeUrl == "https://shopa.com/optout"), 
            It.IsAny<CancellationToken>()), 
            Times.Once);

        repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TenantSettings>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Success_UpdateExistingSettings()
    {
        var tenantId = Guid.NewGuid();
        var existingSettings = new TenantSettings(
            tenantId,
            tenantName: "Old Shop",
            tenantLogoUrl: "https://old.com/logo.png",
            tenantWebsite: "https://old.com",
            tenantAddress: "Old Address",
            unsubscribeUrl: "https://old.com/unsub",
            headerBackgroundColor: "#000000"
        );

        var command = new ConfigureTenantBrandingCommand(
            TenantName: "Shop B",
            TenantLogoUrl: "https://shopb.com/logo.png",
            TenantWebsite: "https://shopb.com",
            TenantAddress: "Street 2",
            UnsubscribeUrl: "https://shopb.com/optout",
            HeaderBackgroundColor: "#ffffff"
        );

        var tenantProviderMock = new Mock<ITenantProvider>();
        tenantProviderMock.Setup(x => x.TenantId).Returns(tenantId);

        var repositoryMock = new Mock<ITenantSettingsRepository>();
        repositoryMock.Setup(x => x.GetByTenantIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSettings);

        var handler = new ConfigureTenantBrandingCommandHandler(repositoryMock.Object, tenantProviderMock.Object);

        await handler.Handle(command, TestContext.Current.CancellationToken);

        repositoryMock.Verify(x => x.UpdateAsync(
            It.Is<TenantSettings>(s => 
                s.TenantId == tenantId && 
                s.TenantName == "Shop B" && 
                s.TenantLogoUrl == "https://shopb.com/logo.png" && 
                s.HeaderBackgroundColor == "#ffffff" && 
                s.TenantAddress == "Street 2" && 
                s.UnsubscribeUrl == "https://shopb.com/optout"), 
            It.IsAny<CancellationToken>()), 
            Times.Once);

        repositoryMock.Verify(x => x.AddAsync(It.IsAny<TenantSettings>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Error_TenantIdRequired()
    {
        var command = new ConfigureTenantBrandingCommand(
            TenantName: "Shop A",
            TenantLogoUrl: "https://shopa.com/logo.png",
            TenantWebsite: "https://shopa.com",
            TenantAddress: "Street 1",
            UnsubscribeUrl: "https://shopa.com/optout"
        );

        var tenantProviderMock = new Mock<ITenantProvider>();
        tenantProviderMock.Setup(x => x.TenantId).Returns((Guid?)null);

        var repositoryMock = new Mock<ITenantSettingsRepository>();

        var handler = new ConfigureTenantBrandingCommandHandler(repositoryMock.Object, tenantProviderMock.Object);

        Task act() => handler.Handle(command, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<UnauthorizedException>(act);

        repositoryMock.Verify(x => x.AddAsync(It.IsAny<TenantSettings>(), It.IsAny<CancellationToken>()), Times.Never);
        repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TenantSettings>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
