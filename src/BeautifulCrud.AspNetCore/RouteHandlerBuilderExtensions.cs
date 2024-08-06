using BeautifulCrud.AspNetCore.EndpointFilters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BeautifulCrud.AspNetCore;

public static class RouteHandlerBuilderExtensions
{
	public static RouteHandlerBuilder CollectionQuery(this RouteHandlerBuilder builder)
	{
		return builder.Project().Filter().Sort().Paging().Count();
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

	public static RouteHandlerBuilder Count(this RouteHandlerBuilder builder)
	{
		builder.AddEndpointFilter<CountEndpointFilter>();
		builder.WithMetadata(new CountEndpointFilter()); 
		return builder;
	}

    public static RouteHandlerBuilder Prefer(this RouteHandlerBuilder builder)
    {
        builder.AddEndpointFilter<PreferEndpointFilter>();
        builder.WithMetadata(new PreferEndpointFilter()); 
        return builder;
    }
}