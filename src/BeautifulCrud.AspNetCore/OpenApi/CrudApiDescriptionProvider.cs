using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace BeautifulCrud.AspNetCore.OpenApi;

public class CrudApiDescriptionProvider : IApiDescriptionProvider
{
    public int Order => int.MaxValue;

    public void OnProvidersExecuting(ApiDescriptionProviderContext context)
    {
        foreach (var description in context.Results)
        {
            var resourceQueryParameters = description.ParameterDescriptions
                .Where(p => p.Type == typeof(ResourceQuery))
                .ToList();

            foreach (var param in resourceQueryParameters)
                description.ParameterDescriptions.Remove(param);
        }
    }

    public void OnProvidersExecuted(ApiDescriptionProviderContext context) { }
}