using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using BeautifulCrud.AspNetCore.ActionFilters;
using BeautifulCrud.AspNetCore.Extensions;
using BeautifulCrud.AspNetCore.Models;
using BeautifulCrud.AspNetCore.OpenApi;
using BeautifulCrud.AspNetCore.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BeautifulCrud.AspNetCore;

public static class ServiceCollectionExtensions
{
    [ExcludeFromCodeCoverage]
    public static IServiceCollection AddBeautifulCrud(this IServiceCollection services, IConfiguration configuration) => services.AddBeautifulCrud(configuration.Bind);

    public static IServiceCollection AddBeautifulCrud(this IServiceCollection services, Action<CrudOptions>? configureAction)
    {
        services.Configure<CrudOptions>(o =>
        {
            configureAction?.Invoke(o);
        });

        var options = new CrudOptions();
        configureAction?.Invoke(options);

        if (options.Features.HasFlagFast(Features.Controllers) || options.Features.HasFlagFast(Features.MinimalApis))
        {
            services.TryAddSingleton<Func<DateTimeOffset>>(_ => () => DateTimeOffset.Now);
            services.TryAddSingleton<IQueryHashEncoder, Base64QueryHashEncoder>();
            services.TryAddSingleton<IResourceQuerySerializer, BinaryResourceQuerySerializer>();
            services.TryAddSingleton<IContinuationTokenGenerator, PortableContinuationTokenGenerator>();
        }

		if (options.Features.HasFlagFast(Features.Controllers))
		{
            services.TryAddEnumerable(ServiceDescriptor.Transient<IApiDescriptionProvider, CrudApiDescriptionProvider>());

			var builder = services.AddMvcCore(o =>
			{
                o.ModelBinderProviders.Insert(0, new ResourceQueryModelBinderProvider());
			});

			builder.AddJsonOptions(o => { SetJsonOptions(o.JsonSerializerOptions); });

			services.TryAddSingleton<ProjectActionFilter>();
			services.TryAddSingleton<FilterActionFilter>();
			services.TryAddSingleton<SortingActionFilter>();
			services.TryAddSingleton<PagingActionFilter>();
			services.TryAddSingleton<CountActionFilter>();
            services.TryAddSingleton<CollectionQueryActionFilter>();
            services.TryAddSingleton<PreferActionFilter>();
		}

		if (options.Features.HasFlagFast(Features.MinimalApis))
		{
			services.Configure<JsonOptions>(o => { SetJsonOptions(o.SerializerOptions); });
		}

		if (options.Features.HasFlagFast(Features.OpenApi))
		{
            services.TryAddScoped<CrudOperationFilter>();
			services.AddSwaggerGen(c =>
			{
                c.OperationFilter<CrudOperationFilter>();
			});
		}

		return services;
	}

    private static void SetJsonOptions(JsonSerializerOptions options)
    {
        if (!options.Converters.OfType<ExpandoObjectConverter>().Any())
            options.Converters.Add(new ExpandoObjectConverter(options.PropertyNamingPolicy));

        if (!options.Converters.OfType<ManyConverterFactory>().Any())
            options.Converters.Add(new ManyConverterFactory(options.PropertyNamingPolicy));

        if (!options.Converters.OfType<CountManyConverterFactory>().Any())
            options.Converters.Add(new CountManyConverterFactory(options.PropertyNamingPolicy));

        if (!options.Converters.OfType<OneConverterFactory>().Any())
            options.Converters.Add(new OneConverterFactory(options.PropertyNamingPolicy));

        // 
        // Currently, we're setting the enum to camelCase because it's indicated in the guidelines
        // (though it's shown once as PascalCase, so it's not entirely clear):
        //
        // https://github.com/microsoft/api-guidelines/blob/vNext/Guidelines.md#1323-post-hybrid-model
        //
        if (!options.Converters.Any(x => x is JsonStringEnumConverter))
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }
}