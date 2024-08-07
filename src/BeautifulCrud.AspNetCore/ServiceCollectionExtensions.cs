using BeautifulCrud.AspNetCore.ActionFilters;
using BeautifulCrud.AspNetCore.Extensions;
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
            services.TryAdd(ServiceDescriptor.Singleton<Func<DateTimeOffset>>(_ => () => DateTimeOffset.Now));
            services.TryAddSingleton<IQueryStore, OpaqueQueryStore>();
        }

		if (options.Features.HasFlagFast(Features.Controllers))
		{
            services.TryAddEnumerable(ServiceDescriptor.Transient<IApiDescriptionProvider, CrudApiDescriptionProvider>());

			var builder = services.AddMvcCore(o =>
			{
                o.ModelBinderProviders.Insert(0, new ResourceQueryModelBinderProvider());
			});

			builder.AddJsonOptions(o =>
			{
				if(!o.JsonSerializerOptions.Converters.OfType<ExpandoObjectConverter>().Any())
					o.JsonSerializerOptions.Converters.Add(new ExpandoObjectConverter(o.JsonSerializerOptions.PropertyNamingPolicy));

                if(!o.JsonSerializerOptions.Converters.OfType<ManyConverterFactory>().Any())
                    o.JsonSerializerOptions.Converters.Add(new ManyConverterFactory(o.JsonSerializerOptions.PropertyNamingPolicy));

                if(!o.JsonSerializerOptions.Converters.OfType<CountManyConverterFactory>().Any())
                    o.JsonSerializerOptions.Converters.Add(new CountManyConverterFactory(o.JsonSerializerOptions.PropertyNamingPolicy));

                if(!o.JsonSerializerOptions.Converters.OfType<OneConverterFactory>().Any())
                    o.JsonSerializerOptions.Converters.Add(new OneConverterFactory(o.JsonSerializerOptions.PropertyNamingPolicy));
			});

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
			services.Configure<JsonOptions>(o =>
			{
				if (!o.SerializerOptions.Converters.OfType<ExpandoObjectConverter>().Any())
					o.SerializerOptions.Converters.Add(new ExpandoObjectConverter(o.SerializerOptions.PropertyNamingPolicy));

                if (!o.SerializerOptions.Converters.OfType<ManyConverterFactory>().Any())
                    o.SerializerOptions.Converters.Add(new ManyConverterFactory(o.SerializerOptions.PropertyNamingPolicy));

                if (!o.SerializerOptions.Converters.OfType<CountManyConverterFactory>().Any())
                    o.SerializerOptions.Converters.Add(new CountManyConverterFactory(o.SerializerOptions.PropertyNamingPolicy));

                if (!o.SerializerOptions.Converters.OfType<OneConverterFactory>().Any())
                    o.SerializerOptions.Converters.Add(new OneConverterFactory(o.SerializerOptions.PropertyNamingPolicy));
			});
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
}