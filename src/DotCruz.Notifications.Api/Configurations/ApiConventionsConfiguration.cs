using DotCruz.Notifications.Api.Filters;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DotCruz.Notifications.Api.Configurations;

public static class ApiConventionsConfiguration
{
    public static IMvcBuilder AddApiConventions(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
            options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services
            .AddControllers(options =>
            {
                options.Filters.Add<ExceptionFilter>();

                options.Conventions.Add(
                    new RouteTokenTransformerConvention(new KebabCaseParameterTransformer()));

                ReplaceQueryValueProvider(options.ValueProviderFactories);
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
    }

    private static void ReplaceQueryValueProvider(IList<IValueProviderFactory> factories)
    {
        var defaultFactory = factories.OfType<QueryStringValueProviderFactory>().FirstOrDefault();

        if (defaultFactory is null)
        {
            factories.Add(new SnakeCaseQueryValueProviderFactory());
            return;
        }

        var index = factories.IndexOf(defaultFactory);
        factories[index] = new SnakeCaseQueryValueProviderFactory();
    }
}

public sealed partial class KebabCaseParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        var input = value?.ToString();

        return string.IsNullOrEmpty(input)
            ? input
            : WordBoundaryRegex().Replace(input, "$1-$2").ToLowerInvariant();
    }

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex WordBoundaryRegex();
}

public sealed class SnakeCaseQueryValueProviderFactory : IValueProviderFactory
{
    public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var query = context.ActionContext.HttpContext.Request.Query;
        context.ValueProviders.Add(
            new SnakeCaseQueryValueProvider(BindingSource.Query, query, CultureInfo.InvariantCulture));

        return Task.CompletedTask;
    }
}

public sealed class SnakeCaseQueryValueProvider(
    BindingSource bindingSource,
    IQueryCollection values,
    CultureInfo culture) : QueryStringValueProvider(bindingSource, values, culture)
{
    public override bool ContainsPrefix(string prefix) => base.ContainsPrefix(ToSnakeCase(prefix));

    public override ValueProviderResult GetValue(string key) => base.GetValue(ToSnakeCase(key));

    private static string ToSnakeCase(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return key;
        }

        return string.Join('.', key.Split('.').Select(JsonNamingPolicy.SnakeCaseLower.ConvertName));
    }
}
