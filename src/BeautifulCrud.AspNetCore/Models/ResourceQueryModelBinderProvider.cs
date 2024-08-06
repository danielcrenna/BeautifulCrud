using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BeautifulCrud.AspNetCore;

internal sealed class ResourceQueryModelBinderProvider : IModelBinderProvider
{
	public IModelBinder? GetBinder(ModelBinderProviderContext context)
	{
		var isResourceQuery = typeof(ResourceQuery).IsAssignableFrom(context.Metadata.ModelType);
		return isResourceQuery
			? new ResourceQueryModelBinder()
			: null;
	}
}