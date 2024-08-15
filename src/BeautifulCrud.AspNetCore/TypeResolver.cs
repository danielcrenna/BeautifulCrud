using System.Collections.Concurrent;
using System.Reflection;
using BeautifulCrud.AspNetCore.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace BeautifulCrud.AspNetCore;

internal static class TypeResolver
{
	private static readonly ConcurrentDictionary<object, Type?> TypeCache = new();

    public static Type? LookupType(this ControllerActionDescriptor descriptor) => TypeCache.GetValueOrDefault(descriptor);

    public static Type? LookupType(this Endpoint endpoint) => TypeCache.GetValueOrDefault(GenerateEndpointCacheKey(endpoint));

    public static Type? LookupType(this ActionModel model) => TypeCache.GetValueOrDefault(model);

    public static Type? ResolveType(this EndpointFilterInvocationContext context)
	{
		var endpoint = context.HttpContext.GetEndpoint();
		if (endpoint == null)
			return null;

		if (TypeCache.TryGetValue(endpoint, out var type))
			return type;

		foreach (var metadata in endpoint.Metadata.OfType<MethodInfo>())
		{
			var returnType = metadata.ReturnType;

			if (typeof(Task).IsAssignableFrom(returnType))
                returnType = returnType.IsGenericType ? returnType.GetGenericArguments()[0] : typeof(void);

			type = returnType.IsGenericType ? returnType.GetGenericArguments()[0] : returnType;
		}

		TypeCache.TryAdd(GenerateEndpointCacheKey(endpoint), type);
		return type;
	}

	public static Type? ResolveType<T>(this ActionExecutingContext context)
	{
		var actionDescriptor = context.ActionDescriptor;
		
		if (TypeCache.TryGetValue(actionDescriptor, out var type))
			return type;

        if (type == null && context.Controller is IHasType typed)
            type = typed.Type;

		if (type == null)
        {
            var attribute = actionDescriptor.EndpointMetadata.OfType<CrudFilterAttribute<T>>().FirstOrDefault();
            type = attribute?.Type;
        }

		if (type == null && actionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
		{
			var returnType = controllerActionDescriptor.MethodInfo.ReturnType;

			if (typeof(Task).IsAssignableFrom(returnType))
                returnType = returnType.IsGenericType ? returnType.GetGenericArguments()[0] : typeof(void);

			type = returnType.IsGenericType ? returnType.GetGenericArguments()[0] : returnType;
		}
        
		TypeCache.TryAdd(actionDescriptor, type);
		return type;
	}

    public static Type? ResolveType<T>(this ActionModel model)
    {
        var attribute = model.Attributes.OfType<CrudFilterAttribute<T>>().FirstOrDefault();
        if (attribute == null)
            return null;

        if (TypeCache.TryGetValue(model, out var type))
            return type;

        type = attribute.Type;

        if (type == null && model.Controller.ControllerType.IsAssignableTo(typeof(IHasType)))
        {
            var instance = Activator.CreateInstance(model.Controller.ControllerType);
            if (instance != null)
            {
                var typed = (IHasType) instance;
                type = typed.Type;
            }
        }
        
        if (type == null)
        {
            var returnType = model.ActionMethod.ReturnType;

            if (typeof(Task).IsAssignableFrom(returnType))
                returnType = returnType.IsGenericType ? returnType.GetGenericArguments()[0] : typeof(void);

            type = returnType.IsGenericType ? returnType.GetGenericArguments()[0] : returnType;
        }
        
        TypeCache.TryAdd(model, type);
        return type;
    }

	/// <summary> Endpoint instances are re-generated, and can't be used as a cache key </summary>
    private static string GenerateEndpointCacheKey(Endpoint endpoint)
    {
        if (endpoint is not RouteEndpoint route)
            return endpoint.DisplayName ?? "UnknownEndpoint";

        var routePattern = route.RoutePattern.RawText?.ToLowerInvariant();
        var httpMethods = route.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods;
        var methods = httpMethods != null ? string.Join(",", httpMethods.OrderBy(x => x)) : "ANY";
        return $"{routePattern}:{methods}";
    }
}