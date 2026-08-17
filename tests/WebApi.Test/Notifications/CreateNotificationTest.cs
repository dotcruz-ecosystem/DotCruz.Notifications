using CommonTestUtilities.Commands.Notifications;
using CommonTestUtilities.InlineData;
using DotCruz.Notifications.Application.DTOs.Base;
using DotCruz.Notifications.Contracts.Enums.Notifications;
using DotCruz.Notifications.Domain.Exceptions.Resources;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;

namespace WebApi.Test.Notifications;

public class CreateNotificationTest : NotificationClassFixture
{
    private readonly string ENDPOINT = "api/notification";
    private readonly string _apiToken;
    private readonly string _templateCode;

    public CreateNotificationTest(CustomWebApplicationFactory factory) : base(factory) 
    {
        _apiToken = factory.GetApiToken();
        _templateCode = factory.GetTemplateCode();
    }

    [Theory]
    [ClassData(typeof(IntegrationNotificationTypeInlineDataTest))]
    public async Task Success(IntegrationNotificationType type)
    {
        var command = CreateNotificationCommandBuilder.Build(type: type);

        var response = await DoPost(ENDPOINT, command.Message, _apiToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var responseId = await response.Content.ReadFromJsonAsync<Guid>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotEqual(Guid.Empty, responseId);
    }

    [Theory]
    [ClassData(typeof(IntegrationNotificationTypeInlineDataTest))]
    public async Task Success_With_Template(IntegrationNotificationType type)
    {
        var command = CreateNotificationCommandBuilder.Build(type: type, templateCode: _templateCode);

        var response = await DoPost(ENDPOINT, command.Message, _apiToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var responseId = await response.Content.ReadFromJsonAsync<Guid>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotEqual(Guid.Empty, responseId);
    }

    [Theory]
    [ClassData(typeof(IntegrationNotificationTypeInlineDataTest))]
    public async Task Success_Scheduled(IntegrationNotificationType type)
    {
        var command = CreateNotificationCommandBuilder.Build(type: type, scheduledFor: DateTimeOffset.UtcNow.AddDays(1));

        var response = await DoPost(ENDPOINT, command.Message, _apiToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var responseId = await response.Content.ReadFromJsonAsync<Guid>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotEqual(Guid.Empty, responseId);
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Unauthorized_Request(string culture)
    {
        var command = CreateNotificationCommandBuilder.Build();

        var response = await DoPost(ENDPOINT, command.Message, culture: culture);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Invalid_Token(string culture)
    {
        var command = CreateNotificationCommandBuilder.Build();

        var response = await DoPost(ENDPOINT, command.Message, token: "invalidToken", culture: culture);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Empty_Recipient(string culture)
    {
        var command = CreateNotificationCommandBuilder.Build(recipient: string.Empty);

        var response = await DoPost(ENDPOINT, command.Message, token: _apiToken, culture: culture);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseData = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(cancellationToken: TestContext.Current.CancellationToken);

        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("RECIPIENT_EMPTY", new CultureInfo(culture));

        Assert.NotNull(responseData);
        Assert.Contains(expectedMessage, responseData.Errors);
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Body_And_Template_Empty(string culture)
    {
        var command = CreateNotificationCommandBuilder.Build(body: string.Empty, templateCode: null);

        var response = await DoPost(ENDPOINT, command.Message, token: _apiToken, culture: culture);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseData = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(cancellationToken: TestContext.Current.CancellationToken);

        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("BODY_OR_TEMPLATE_REQUIRED", new CultureInfo(culture));

        Assert.NotNull(responseData);
        Assert.Contains(expectedMessage, responseData.Errors);
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Invalid_NotificationType(string culture)
    {
        var command = CreateNotificationCommandBuilder.Build(type: (IntegrationNotificationType)100);

        var response = await DoPost(ENDPOINT, command.Message, token: _apiToken, culture: culture);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseData = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(cancellationToken: TestContext.Current.CancellationToken);

        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("NOTIFICATION_TYPE_INVALID", new CultureInfo(culture));

        Assert.NotNull(responseData);
        Assert.Contains(expectedMessage, responseData.Errors);
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Empty_Title_Email(string culture)
    {
        var command = CreateNotificationCommandBuilder.Build(type: IntegrationNotificationType.Email, title: string.Empty);

        var response = await DoPost(ENDPOINT, command.Message, token: _apiToken, culture: culture);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseData = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(cancellationToken: TestContext.Current.CancellationToken);

        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("TITLE_EMPTY", new CultureInfo(culture));

        Assert.NotNull(responseData);
        Assert.Contains(expectedMessage, responseData.Errors);
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Empty_Title_Push(string culture)
    {
        var command = CreateNotificationCommandBuilder.Build(type: IntegrationNotificationType.Push, title: string.Empty);

        var response = await DoPost(ENDPOINT, command.Message, token: _apiToken, culture: culture);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseData = await response.Content.ReadFromJsonAsync<ErrorResponseDto>(cancellationToken: TestContext.Current.CancellationToken);

        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("TITLE_EMPTY", new CultureInfo(culture));

        Assert.NotNull(responseData);
        Assert.Contains(expectedMessage, responseData.Errors);
    }

    [Fact]
    public async Task Error_Missing_Tenant_Header()
    {
        var command = CreateNotificationCommandBuilder.Build(type: IntegrationNotificationType.Email);

        var response = await DoPost(ENDPOINT, command.Message, token: _apiToken, includeTenant: false);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
