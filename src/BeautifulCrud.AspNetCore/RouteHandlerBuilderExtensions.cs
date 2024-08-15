using System.Net.Mime;
using BeautifulCrud.AspNetCore.EndpointFilters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BeautifulCrud.AspNetCore;

public static class RouteHandlerBuilderExtensions
{
	public static RouteHandlerBuilder CollectionQuery<T>(this RouteHandlerBuilder builder)
    {
		builder.Produces(StatusCodes.Status200OK, typeof(Many<T>), MediaTypeNames.Application.Json);

        return builder
            .Project()
            .Filter()
            .Sort()
            .Paging()
            .Count<T>();
	}

    public static RouteHandlerBuilder ItemQuery<T>(this RouteHandlerBuilder builder)
    {
        builder.Produces(StatusCodes.Status200OK, typeof(One<T>), MediaTypeNames.Application.Json);
        builder.Produces(StatusCodes.Status404NotFound, typeof(ProblemDetails), MediaTypeNames.Application.Json);

        return builder.Project();
    }
	
	public static RouteHandlerBuilder Project(this RouteHandlerBuilder builder)
	{
		builder.AddEndpointFilter<ProjectEndpointFilter>();
		builder.WithMetadata(new ProjectEndpointFilter());
        
		return builder;
	}

	public static RouteHandlerBuilder Filter(this RouteHandlerBuilder builder)
	{
		builder.AddEndpointFilter<FilterEndpointFilter>();
		builder.WithMetadata(new FilterEndpointFilter()); 
		return builder;
	}

	public static RouteHandlerBuilder Sort(this RouteHandlerBuilder builder)
	{
		builder.AddEndpointFilter<SortingEndpointFilter>();
		builder.WithMetadata(new SortingEndpointFilter()); 
		return builder;
	}

	public static RouteHandlerBuilder Paging(this RouteHandlerBuilder builder)
	{
		builder.AddEndpointFilter<PagingEndpointFilter>();
		builder.WithMetadata(new PagingEndpointFilter()); 
		return builder;
	}

	public static RouteHandlerBuilder Count<T>(this RouteHandlerBuilder builder)
	{
        builder.Produces(StatusCodes.Status200OK, typeof(CountMany<T>), MediaTypeNames.Application.Json);

		builder.AddEndpointFilter<CountEndpointFilter>();
		builder.WithMetadata(new CountEndpointFilter()); 
		return builder;
	}

    public static RouteHandlerBuilder Prefer(this RouteHandlerBuilder builder)
    {
        builder.Produces(StatusCodes.Status204NoContent);
        builder.Produces(StatusCodes.Status404NotFound, typeof(ProblemDetails), MediaTypeNames.Application.Json);

        builder.AddEndpointFilter<PreferEndpointFilter>();
        builder.WithMetadata(new PreferEndpointFilter()); 
        return builder;
    }
}