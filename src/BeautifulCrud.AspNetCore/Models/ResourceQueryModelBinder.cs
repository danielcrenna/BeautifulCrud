using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BeautifulCrud.AspNetCore.Models;

internal sealed class ResourceQueryModelBinder : IModelBinder
{
	public Task BindModelAsync(ModelBindingContext context)
	{
		context.Result = ModelBindingResult.Success(context.HttpContext.GetResourceQuery());
		return Task.CompletedTask;
	}
}